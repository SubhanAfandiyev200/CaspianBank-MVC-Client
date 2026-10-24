using CaspianBank_MVC_FinalProject.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Formada göstərilən xəta mətnləri (API-dən gələn və ya controller-də yazılan ingilis cümlələr) səhifə göstərilməzdən əvvəl
    // seçilmiş dilə tərcümə olunur. Lüğətdə olmayan mətn olduğu kimi qalır
    public class TranslateModelErrorsFilter : IResultFilter
    {
        private readonly ITranslator _translator;

        public TranslateModelErrorsFilter(ITranslator translator)
        {
            _translator = translator;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (_translator.Lang == "en")
            {
                return;
            }

            foreach (var entry in context.ModelState.Values)
            {
                if (entry.Errors.Count == 0)
                {
                    continue;
                }

                var messages = entry.Errors.Select(e => e.ErrorMessage).ToList();
                entry.Errors.Clear();
                foreach (var message in messages)
                {
                    entry.Errors.Add(string.IsNullOrEmpty(message) ? message : _translator[message]);
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
