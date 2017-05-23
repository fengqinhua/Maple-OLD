//using System;
//using System.Threading.Tasks;
//using Owin;

//namespace Maple.Web.Owin {
//    /// <summary>
//    /// A special Owin middleware that is executed last in the Owin pipeline and runs the non-Owin part of the request.
//    /// </summary>
//    public static class MapleMiddleware {
//        public static IAppBuilder UseMaple(this IAppBuilder app) {
//            app.Use(async (context, next) => {
//                var handler = context.Environment["maple.Handler"] as Func<Task>;
//                if (handler == null) {
//                    throw new ArgumentException("maple.Handler can't be null");
//                }
//                await handler();
//            });

//            return app;
//        }
//    }
//}
