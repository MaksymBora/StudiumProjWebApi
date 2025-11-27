public sealed class AlreadyRatedException : Exception
{
    public AlreadyRatedException()
        : base("User has already rated this product.") { }
}
