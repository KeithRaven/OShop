﻿using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using OShop.Models;
using OShop.Services;


namespace OShop.Handlers {
    [OrchardFeature("OShop.Customers")]
    public class CustomerOrderPartHandler : ContentHandler {
        private ICustomersService _customersService;

        public CustomerOrderPartHandler(
            ICustomersService customersService,
            IRepository<CustomerOrderPartRecord> repository
            ) {
            _customersService = customersService;

            Filters.Add(StorageFilter.For(repository));

            OnActivated<CustomerOrderPart>((context, part) => {
                part._customer.Loader(customer => _customersService.GetCustomer(part.CustomerId));
                part._customer.Setter(customer => {
                    part.CustomerId = customer.Id;
                    return customer;
                });

                part._billingAddress.Loader(address => _customersService.GetAddress(part.BillingAddressId));
                part._billingAddress.Setter(address => {
                    part.BillingAddressId = address.Id;
                    return address;
                });

                part._shippingAddress.Loader(address => _customersService.GetAddress(part.ShippingAddressId));
                part._shippingAddress.Setter(address => {
                    part.ShippingAddressId = address.Id;
                    return address;
                });
            });
        }
    }
}