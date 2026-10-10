namespace CaspianBank_MVC_FinalProject.ViewModels.Common
{
    // Xəta səhifəsində göstərilən məlumat: status koduna görə başlıq və izahat
    public class ErrorVM
    {
        public ErrorVM(int status)
        {
            Status = status;

            if (status == 404)
            {
                Title = "Page not found";
                Message = "We could not find what you were looking for. It may have been removed, or the address may be wrong.";
            }
            else if (status == 403)
            {
                Title = "Access denied";
                Message = "You do not have permission to open this page.";
            }
            else if (status == 401)
            {
                Title = "Please log in";
                Message = "You need to log in to open this page.";
            }
            else if (status >= 400 && status < 500)
            {
                Title = "We could not process that";
                Message = "The request was not valid. Go back and try again.";
            }
            else
            {
                Title = "Something went wrong";
                Message = "An unexpected error happened on our side. Please try again in a moment.";
            }
        }

        public int Status { get; }
        public string Title { get; }
        public string Message { get; }
    }
}
