using System;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using OShop.Events;
using OShop.Models;

namespace OShop.Handlers {
    [OrchardFeature("OShop.Stocks")]
    public class StocksOrderEventHandler : IDependency, IOrderEventHandler {
        private readonly IContentManager _contentManager;
        private readonly OrderStatus _inOrderStockAllocationStatus = OrderStatus.Processing;

        public StocksOrderEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void OrderCanceled(IContent order) {
        }

        public void OrderCompleted(IContent order) {
        }

        public void OrderProcessing(IContent order) {
        }

        public void OrderCreated(IContent order) {
        }

        public void OrderDetailCreated(IContent order, OrderDetailRecord createdDetail) {
            var stockPart = _contentManager.Get(createdDetail.ContentId).As<StockPart>();
            var orderPart = order.As<OrderPart>();
            if(stockPart != null && stockPart.EnableStockMgmt && orderPart != null) {
                if(orderPart.OrderStatus == OrderStatus.Canceled) {
                    return;
                }
                else if(orderPart.OrderStatus < OrderStatus.Completed) {

                    if (orderPart.OrderStatus >= _inOrderStockAllocationStatus) {
                        stockPart.InOrderQty += createdDetail.Quantity;
                    }
                    
                }
                else {
                    stockPart.InStockQty -= createdDetail.Quantity;
                }
            }
        }

        public void OrderDetailDeleted(IContent order, OrderDetailRecord deletedDetail) {
            var stockPart = _contentManager.Get(deletedDetail.ContentId).As<StockPart>();
            var orderPart = order.As<OrderPart>();
            if (stockPart != null && stockPart.EnableStockMgmt && orderPart != null) {
                if (orderPart.OrderStatus == OrderStatus.Canceled) {
                    return;
                }
                else if (orderPart.OrderStatus < OrderStatus.Completed) {

                    if (orderPart.OrderStatus >= _inOrderStockAllocationStatus) {
                        stockPart.InOrderQty -= deletedDetail.Quantity;
                    }

                }
                else {
                    stockPart.InStockQty += deletedDetail.Quantity;
                }
            }
        }

        public void OrderDetailUpdated(IContent order, OrderDetailRecord originalDetail, OrderDetailRecord updatedDetail) {
            if(originalDetail.ContentId == updatedDetail.ContentId) {
                var orderPart = order.As<OrderPart>();
                var stockPart = _contentManager.Get(updatedDetail.ContentId).As<StockPart>();
                if (stockPart != null && stockPart.EnableStockMgmt && orderPart != null) {
                    if(orderPart.OriginalStatus != orderPart.OrderStatus) {
                        // OrderStatus changed
                        if (orderPart.OriginalStatus == OrderStatus.Canceled) {
                            if(orderPart.OrderStatus < OrderStatus.Completed) {

                                if (orderPart.OrderStatus >= _inOrderStockAllocationStatus) {
                                    stockPart.InOrderQty += updatedDetail.Quantity;
                                }

                            }
                            else {
                                stockPart.InStockQty -= updatedDetail.Quantity;
                            }
                        }
                        else if (orderPart.OriginalStatus < OrderStatus.Completed) {
                            if (orderPart.OrderStatus == OrderStatus.Canceled) {

                                //deallocate previously allocated stock as now cancelled
                                if (orderPart.OriginalStatus >= _inOrderStockAllocationStatus) {
                                    stockPart.InOrderQty -= originalDetail.Quantity;
                                }
                            }
                            else if (orderPart.OrderStatus == OrderStatus.Completed) {

                                //deallocated previously allocated stock and now taken form stock
                                if (orderPart.OriginalStatus >= _inOrderStockAllocationStatus) {
                                    stockPart.InOrderQty -= originalDetail.Quantity;
                                }

                                    stockPart.InStockQty -= updatedDetail.Quantity;
                            }
                            else {

                                if (orderPart.OrderStatus >= _inOrderStockAllocationStatus) {

                                    if (orderPart.OriginalStatus < _inOrderStockAllocationStatus) {
                                        //previously not allocated but now allocated
                                        stockPart.InOrderQty += updatedDetail.Quantity;
                                    }
                                    else {
                                        //some prevsiously allocated, now allocate the diff
                                        stockPart.InOrderQty += updatedDetail.Quantity - originalDetail.Quantity;
                                    }
                                }
                                else {

                                    //deallocate previously allocated stock
                                    if (orderPart.OriginalStatus >= _inOrderStockAllocationStatus) {
                                        stockPart.InOrderQty -= originalDetail.Quantity;
                                    }
  
                              }

                            }
                        }
                        else {
                            stockPart.InStockQty += originalDetail.Quantity;
                            if (orderPart.OrderStatus != OrderStatus.Canceled) {

                                if (orderPart.OrderStatus >= _inOrderStockAllocationStatus) {
                                    stockPart.InOrderQty += updatedDetail.Quantity;
                                }
                                
                            }
                        }
                    }
                    else {
                        // OrderStatus unchanged
                        if (originalDetail.Quantity != updatedDetail.Quantity) {
                            stockPart.InOrderQty += updatedDetail.Quantity - originalDetail.Quantity;
                        }
                    }
                }
            }
            else {
                OrderDetailDeleted(order, originalDetail);
                OrderDetailCreated(order, updatedDetail);
            }
        }
    }
}