using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace OShop.Activities {
    [OrchardFeature("OShop.Orders.Workflows")]
    public class OrderProcessingActivity : OrderActivity {
        public const string EventName = "OrderProcessing";

        public override string Name {
            get { return EventName; }
        }

        public override LocalizedString Description {
            get { return T("An order is set to processing (i.e. has been paid)."); }
        }
    }
}