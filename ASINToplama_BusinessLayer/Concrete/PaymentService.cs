using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_EntityLayer.Concrete;

namespace ASINToplama_BusinessLayer.Concrete
{
    public class PaymentService : GenericService<Payment>, IPaymentService
    {
        public PaymentService(IGenericRepository<Payment> payments, IUnitOfWork uow) : base(payments, uow) { }
    }
}
