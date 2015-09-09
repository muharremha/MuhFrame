using Muh.Core;
using Muh.Core.Caching;
using Muh.Core.Data;
using Muh.Core.Domain.Common;
using Muh.Core.Domain.Customers;
using Muh.Data;
using Muh.Services.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Muh.Services.Customers
{
    public partial class CustomerService :ICustomerService
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        private const string CUSTOMERROLES_ALL_KEY = "Muh.customerrole.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : system name
        /// </remarks>
        private const string CUSTOMERROLES_BY_SYSTEMNAME_KEY = "Muh.customerrole.systemname-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CUSTOMERROLES_PATTERN_KEY = "Muh.customerrole.";

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly CommonSettings _commonSettings;
     


        public CustomerService(ICacheManager cacheManager,
            IRepository<Customer> customerRepository,
              IRepository<CustomerRole> customerRoleRepository,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            CommonSettings commonSettings,
            CustomerSettings customers)
        {
            _cacheManager = cacheManager;
            _customerRepository = customerRepository;
            _customerRoleRepository = customerRoleRepository;
            _dataProvider = dataProvider;
            _dbContext = dbContext;
            _commonSettings = commonSettings;
            _eventPublisher = eventPublisher;

        }


        /// <summary>
        /// Insert a guest customer
        /// </summary>
        /// <returns>Customer</returns>
        public virtual Customer InsertGuestCustomer()
        {
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            //add to 'Guests' role
            var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
            if (guestRole == null)
                throw new NopException("'Guests' role could not be loaded");
            customer.CustomerRoles.Add(guestRole);

            _customerRepository.Insert(customer);

            return customer;
        }



        /// <summary>
        /// Delete guest customer records
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete customers only without shopping cart</param>
        /// <returns>Number of deleted customers</returns>
        public virtual int DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, bool onlyWithoutShoppingCart)
        {
            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //stored procedures are enabled and supported by the database. 
                //It's much faster than the LINQ implementation below 

                #region Stored procedure

                //prepare parameters
                var pOnlyWithoutShoppingCart = _dataProvider.GetParameter();
                pOnlyWithoutShoppingCart.ParameterName = "OnlyWithoutShoppingCart";
                pOnlyWithoutShoppingCart.Value = onlyWithoutShoppingCart;
                pOnlyWithoutShoppingCart.DbType = DbType.Boolean;

                var pCreatedFromUtc = _dataProvider.GetParameter();
                pCreatedFromUtc.ParameterName = "CreatedFromUtc";
                pCreatedFromUtc.Value = createdFromUtc.HasValue ? (object)createdFromUtc.Value : DBNull.Value;
                pCreatedFromUtc.DbType = DbType.DateTime;

                var pCreatedToUtc = _dataProvider.GetParameter();
                pCreatedToUtc.ParameterName = "CreatedToUtc";
                pCreatedToUtc.Value = createdToUtc.HasValue ? (object)createdToUtc.Value : DBNull.Value;
                pCreatedToUtc.DbType = DbType.DateTime;

                var pTotalRecordsDeleted = _dataProvider.GetParameter();
                pTotalRecordsDeleted.ParameterName = "TotalRecordsDeleted";
                pTotalRecordsDeleted.Direction = ParameterDirection.Output;
                pTotalRecordsDeleted.DbType = DbType.Int32;

                //invoke stored procedure
                _dbContext.ExecuteSqlCommand(
                    "EXEC [DeleteGuests] @OnlyWithoutShoppingCart, @CreatedFromUtc, @CreatedToUtc, @TotalRecordsDeleted OUTPUT",
                    false, null,
                    pOnlyWithoutShoppingCart,
                    pCreatedFromUtc,
                    pCreatedToUtc,
                    pTotalRecordsDeleted);

                int totalRecordsDeleted = (pTotalRecordsDeleted.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecordsDeleted.Value) : 0;
                return totalRecordsDeleted;

                #endregion
            }
            else
            {
                return 0;
            }

            
            //TODO: Muh_sonra bak
/*
            else
            {
                //stored procedures aren't supported. Use LINQ

                #region No stored procedure

                var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
                if (guestRole == null)
                    throw new NopException("'Guests' role could not be loaded");

                var query = _customerRepository.Table;
                if (createdFromUtc.HasValue)
                    query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
                if (createdToUtc.HasValue)
                    query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
                query = query.Where(c => c.CustomerRoles.Select(cr => cr.Id).Contains(guestRole.Id));
                if (onlyWithoutShoppingCart)
                    query = query.Where(c => !c.ShoppingCartItems.Any());
                //no orders
                query = from c in query
                        join o in _orderRepository.Table on c.Id equals o.CustomerId into c_o
                        from o in c_o.DefaultIfEmpty()
                        where !c_o.Any()
                        select c;
                //no blog comments
                query = from c in query
                        join bc in _blogCommentRepository.Table on c.Id equals bc.CustomerId into c_bc
                        from bc in c_bc.DefaultIfEmpty()
                        where !c_bc.Any()
                        select c;
                //no news comments
                query = from c in query
                        join nc in _newsCommentRepository.Table on c.Id equals nc.CustomerId into c_nc
                        from nc in c_nc.DefaultIfEmpty()
                        where !c_nc.Any()
                        select c;
                //no product reviews
                query = from c in query
                        join pr in _productReviewRepository.Table on c.Id equals pr.CustomerId into c_pr
                        from pr in c_pr.DefaultIfEmpty()
                        where !c_pr.Any()
                        select c;
                //no product reviews helpfulness
                query = from c in query
                        join prh in _productReviewHelpfulnessRepository.Table on c.Id equals prh.CustomerId into c_prh
                        from prh in c_prh.DefaultIfEmpty()
                        where !c_prh.Any()
                        select c;
                //no poll voting
                query = from c in query
                        join pvr in _pollVotingRecordRepository.Table on c.Id equals pvr.CustomerId into c_pvr
                        from pvr in c_pvr.DefaultIfEmpty()
                        where !c_pvr.Any()
                        select c;
                //no forum posts 
                query = from c in query
                        join fp in _forumPostRepository.Table on c.Id equals fp.CustomerId into c_fp
                        from fp in c_fp.DefaultIfEmpty()
                        where !c_fp.Any()
                        select c;
                //no forum topics
                query = from c in query
                        join ft in _forumTopicRepository.Table on c.Id equals ft.CustomerId into c_ft
                        from ft in c_ft.DefaultIfEmpty()
                        where !c_ft.Any()
                        select c;
                //don't delete system accounts
                query = query.Where(c => !c.IsSystemAccount);

                //only distinct customers (group by ID)
                query = from c in query
                        group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                query = query.OrderBy(c => c.Id);
                var customers = query.ToList();


                int totalRecordsDeleted = 0;
                foreach (var c in customers)
                {
                    try
                    {
                        //delete attributes
                        var attributes = _genericAttributeService.GetAttributesForEntity(c.Id, "Customer");
                        foreach (var attribute in attributes)
                            _genericAttributeService.DeleteAttribute(attribute);


                        //delete from database
                        _customerRepository.Delete(c);
                        totalRecordsDeleted++;
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc);
                    }
                }
                return totalRecordsDeleted;

                #endregion
            }

            */
        }

        

        /// <summary>
        /// Get customer by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Customer</returns>
        public virtual Customer GetCustomerBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.SystemName == systemName
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        


        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="systemName">Customer role system name</param>
        /// <returns>Customer role</returns>
        public virtual CustomerRole GetCustomerRoleBySystemName(string systemName)
        {
            if (String.IsNullOrWhiteSpace(systemName))
                return null;

            string key = string.Format(CUSTOMERROLES_BY_SYSTEMNAME_KEY, systemName);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _customerRoleRepository.Table
                            orderby cr.Id
                            where cr.SystemName == systemName
                            select cr;
                var customerRole = query.FirstOrDefault();
                return customerRole;
            });
        }

        public Core.IPagedList<Customer> GetAllCustomers(string email = null, string username = null, int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = _customerRepository.Table;
            if (!String.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!String.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));

            query = query.Where(c => c.Email !=null);

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var customers = new PagedList<Customer>(query, pageIndex, pageSize);
            return customers;

        }

        public void DeleteCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Delete(customer);



        }

        public Customer GetCustomerById(int customerId)
        {
            if (customerId == 0)
                return null;

            return _customerRepository.GetById(customerId);
        }

        public IList<Customer> GetCustomersByIds(int[] customerIds)
        {
            if (customerIds == null || customerIds.Length == 0)
                return new List<Customer>();

            var query = from c in _customerRepository.Table
                        where customerIds.Contains(c.Id)
                        select c;
            var customers = query.ToList();
            //sort by passed identifiers
            var sortedCustomers = new List<Customer>();
            foreach (int id in customerIds)
            {
                var customer = customers.Find(x => x.Id == id);
                if (customer != null)
                    sortedCustomers.Add(customer);
            }
            return sortedCustomers;
        }

        public Customer GetCustomerByGuid(Guid customerGuid)
        {
            if (customerGuid == Guid.Empty)
                return null;

            var query = from c in _customerRepository.Table
                        where c.CustomerGuid == customerGuid
                        orderby c.Id
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        public Customer GetCustomerByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.Email == email
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }


        public Customer GetCustomerByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.Username == username
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        public void InsertCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Insert(customer);

            //event notification
            _eventPublisher.EntityInserted(customer);
        }

        public void UpdateCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Update(customer);

            //event notification
            _eventPublisher.EntityUpdated(customer);
        }


        #region Customer roles

        /// <summary>
        /// Delete a customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        public virtual void DeleteCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            if (customerRole.IsSystemRole)
                throw new NopException("System role could not be deleted");

            _customerRoleRepository.Delete(customerRole);

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(customerRole);
        }

        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="customerRoleId">Customer role identifier</param>
        /// <returns>Customer role</returns>
        public virtual CustomerRole GetCustomerRoleById(int customerRoleId)
        {
            if (customerRoleId == 0)
                return null;

            return _customerRoleRepository.GetById(customerRoleId);
        }

       
        /// <summary>
        /// Gets all customer roles
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Customer roles</returns>
        public virtual IList<CustomerRole> GetAllCustomerRoles(bool showHidden = false)
        {
            string key = string.Format(CUSTOMERROLES_ALL_KEY, showHidden);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _customerRoleRepository.Table
                            orderby cr.Name
                            where (showHidden || cr.Active)
                            select cr;
                var customerRoles = query.ToList();
                return customerRoles;
            });
        }

        /// <summary>
        /// Inserts a customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        public virtual void InsertCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            _customerRoleRepository.Insert(customerRole);

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(customerRole);
        }

        /// <summary>
        /// Updates the customer role
        /// </summary>
        /// <param name="customerRole">Customer role</param>
        public virtual void UpdateCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            _customerRoleRepository.Update(customerRole);

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(customerRole);
        }

        #endregion
    }
}
