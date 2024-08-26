namespace ControleCerto.Errors
{
    public class Result<T>
    {
        private readonly T? _value;
        private readonly AppError? _error;
        private readonly bool _isSuccess;

        public Result(T value)
        {
            _value = value;
            _isSuccess = true;
            _error = null;
        }

        public Result(AppError appError)
        {
            _error = appError;
            _isSuccess = false;
        }

        public static implicit operator Result<T>(T value) => new(value);
        public static implicit operator Result<T>(AppError error) => new(error);

        public bool IsSuccess => _isSuccess;
        public bool IsError => !_isSuccess;

        public T Value => _isSuccess ? _value! : throw new InvalidOperationException("Result is an error");
        public AppError Error => !_isSuccess ? _error! : throw new InvalidOperationException("Result is a success");

        public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<AppError, TResult> onError)
        {
            return _isSuccess ? onSuccess(_value!) : onError(_error!);
        }

        public override string ToString() => _isSuccess ? _value!.ToString() ?? "" : _error!.ErrorMessage ?? "";
    }
}
