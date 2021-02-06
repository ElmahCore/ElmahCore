namespace ElmahCore.Assertions
{
    /// <summary>
    ///     Provides evaluation of a context to determine whether it matches
    ///     certain criteria or not.
    /// </summary>
    internal interface IAssertion
    {
        /// <remarks>
        ///     The context is typed generically as System.Object when it could have
        ///     been restricted to System.Web.HttpContext and also avoid unnecessary
        ///     casting downstream. However, using object allows simple
        ///     assertions to be unit-tested without having to stub out a lot of
        ///     the classes from System.Web (most of which cannot be stubbed anyhow
        ///     due to lack of virtual and instance methods).
        /// </remarks>
        bool Test(object context);
    }
}