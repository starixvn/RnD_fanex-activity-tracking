namespace Fanex.ActivityTracking.LogCollector.Models
{
    public class Result
    {
        public const int ErrorCode = 1;
        public static readonly Result SuccessResult = new Result();

        public int Code { get; set; }

        public string Message { get; set; }

        public virtual bool IsSuccessful
        {
            get
            {
                return Code == 0;
            }
        }

        public virtual bool IsFailed
        {
            get
            {
                return !IsSuccessful;
            }
        }

        public virtual bool IsOk
        {
            get
            {
                return Code == 0;
            }
        }

        public virtual bool IsNotOk
        {
            get
            {
                return !IsOk;
            }
        }

        public static Result CreateErrorResult(string message, int? code = null)
        {
            return new Result { Code = code.HasValue ? code.Value : ErrorCode, Message = message };
        }
    }
}