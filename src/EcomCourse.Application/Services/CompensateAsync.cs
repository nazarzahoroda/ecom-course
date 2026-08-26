//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace EcomCourse.Application.Services
//{
//    internal class CompensateAsync
//    {
//        private async Task CompensateAsync(
//    Applica user,
//    Guid customerId,
//    CancellationToken cancellationToken)
//        {
//            if (customerId != Guid.Empty)
//            {
//                var customer =
//                    await _context.Customers
//                        .FirstOrDefaultAsync(
//                            x => x.Id == customerId,
//                            cancellationToken);

//                if (customer is not null)
//                {
//                    _context.Customers.Remove(customer);

//                    await _context.SaveChangesAsync(
//                        cancellationToken);
//                }
//            }

//            await _manager.DeleteAsync(user);
//        }
//    }
//}
