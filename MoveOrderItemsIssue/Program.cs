using Resto.Front.Api;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.UI;

namespace MoveOrderItemsIssue;

[PluginLicenseModuleId(21016318)]
public class Program : IFrontPlugin
{
    private readonly IDisposable? _subscription;
    
    public Program()
    {
        _subscription = PluginContext.Operations.AddButtonToOrderEditScreen("Move order items test", MoveOrderItems);
    }
    
    private void MoveOrderItems((IOrder order, IOperationService os, IViewManager vm) obj)
    {
        try
        {
            var (order, guest) = CreateNewOrderForMoving(obj.os);
            MoveOrderItem(obj, guest, order);
        }
        catch (Exception e)
        {
            obj.vm.ShowErrorPopup(e.Message);
        }
        
    }
    
    private (IOrder order, IOrderGuestItem guest) CreateNewOrderForMoving(IOperationService operationService)
    {
        var editSession = operationService.CreateEditSession();
        var createdOrderStub = editSession.CreateOrder(tables: null, waiter:operationService.GetCurrentUser());
        editSession.AddOrderGuest("Name", createdOrderStub);
        var newOrder = operationService.SubmitChanges(editSession, operationService.AuthenticateByPin("12344321")).Get(createdOrderStub);
        
        return (newOrder, newOrder.Guests.First());
    }
    
    private static void MoveOrderItem((IOrder order, IOperationService os, IViewManager vm) obj, IOrderGuestItem destinationGuest, IOrder destinationOrder)
    {
        var editSession = obj.os.CreateEditSession();
        editSession.MoveOrderItemToAnotherOrder(obj.order.Items.First(), obj.order, destinationGuest, destinationOrder);
        obj.os.SubmitChanges(editSession, obj.os.AuthenticateByPin("12344321"));
    }
    
    public void Dispose()
    {
        _subscription?.Dispose();
    }
}