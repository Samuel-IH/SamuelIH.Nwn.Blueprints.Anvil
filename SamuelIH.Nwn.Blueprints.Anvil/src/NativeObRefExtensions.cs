using Anvil.API;
using NWN.Native.API;
using System.Linq.Expressions;

namespace HackyJunk
{
    // quick reflection extensions that I made in a time crunch

    internal static class NativeObRefExtensions
    {

        // we target this method:
        // internal static NWObject? NwObject.CreateInternal(ICGameObject? gameObject)
        // which is private, so we need to use reflection to get it
        // we also need to cache the method info as a delegate, so we don't have to do the reflection every time


        static System.Reflection.MethodInfo CreateInternalMethod = typeof(NwObject).GetMethod("CreateInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, new System.Type[] { typeof(ICGameObject) })!;
        static System.Func<ICGameObject?, NwObject?> CreateInternalDelegate = (System.Func<ICGameObject?, NwObject?>)System.Delegate.CreateDelegate(typeof(System.Func<ICGameObject?, NwObject?>), CreateInternalMethod);

        // use exprcession tree instead:
        static Expression inParam = Expression.Parameter(typeof(ICGameObject), "in");
        static System.Func<ICGameObject?, NwObject?> CreateInternalDelegate2 = Expression.Lambda<System.Func<ICGameObject?, NwObject?>>(Expression.Call(CreateInternalMethod, inParam), new ParameterExpression[] { (ParameterExpression)inParam }).Compile();


        private static NwObject? CreateInternal(this ICGameObject? gameObject)
        {
            return CreateInternalDelegate(gameObject);
        }

        internal static T? ToNwObject<T>(this ICGameObject gameObject) where T : NwObject
        {
            return (T?)CreateInternal(gameObject);
        }
    }
}