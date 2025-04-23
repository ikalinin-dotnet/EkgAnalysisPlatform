// Create Validators/EkgSignalValidator.cs
using EkgAnalysisPlatform.Core.Models;
using FluentValidation;

public class EkgSignalValidator : AbstractValidator<EkgSignal>
{
    public EkgSignalValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.RecordedAt).NotEmpty();
        RuleFor(x => x.SamplingRate).GreaterThan(0);
        RuleFor(x => x.DataPoints).NotEmpty();
    }
}