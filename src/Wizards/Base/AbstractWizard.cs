using XperienceCommunity.Manager.Options;
using XperienceCommunity.Manager.Steps;

namespace XperienceCommunity.Manager.Wizards.Base
{
    /// <summary>
    /// A configuration wizard which displays steps for user input and populates an <see cref="IWizardOptions"/>
    /// with the provided data.
    /// </summary>
    public abstract class AbstractWizard<TOptions> : IWizard<TOptions> where TOptions : IWizardOptions, new()
    {
        public StepList Steps { get; } = [];


        public TOptions Options { get; set; } = new();


        public abstract Task InitSteps(params string[] args);


        public async Task<TOptions> Run(params string[] args)
        {
            await InitSteps(args);
            if (Steps.Any())
            {
                do
                {
                    await Steps.Current.Execute();
                } while (Steps.Next());
            }

            return Options;
        }
    }
}
