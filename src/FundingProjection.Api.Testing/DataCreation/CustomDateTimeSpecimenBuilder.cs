using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace FundingProjection.Api.Testing.DataCreation;

/*
    This is a customisation of:
    https://github.com/AutoFixture/AutoFixture/blob/master/Src/AutoFixture/RandomDateTimeSequenceGenerator.cs

    Since we use the SQL DateTime data type in the database we lose some precision storing values in the db,
    so this customisation generates date time values without fractional seconds for mocked .Net DateTime
    values. This avoids constantly having to customise the BeEquivalentTo to account for the difference.
 */
public class CustomDateTimeSpecimenBuilder: ISpecimenBuilder
{
    private readonly RandomNumericSequenceGenerator _randomizer;
    private const int RangeInYears = 2;

    public CustomDateTimeSpecimenBuilder() : this(DateTime.Today.AddYears(-RangeInYears), DateTime.Today.AddYears(RangeInYears))
    {}

    private CustomDateTimeSpecimenBuilder(DateTime minDate, DateTime maxDate)
    {
        _randomizer = new RandomNumericSequenceGenerator(minDate.Ticks, maxDate.Ticks);
    }
    
    public object Create(object request, ISpecimenContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return IsNotDateTimeRequest(request)
            ? new NoSpecimen()
            : CreateRandomDate(context);
    }

    private static bool IsNotDateTimeRequest(object request)
    {
        return !typeof(DateTime).GetTypeInfo().IsAssignableFrom(request as Type);
    }

    private object CreateRandomDate(ISpecimenContext context)
    {
        var result = new DateTime(GetRandomNumberOfTicks(context));
        return result.AddTicks(-Convert.ToInt64(result.Ticks % TimeSpan.TicksPerSecond));
    }

    private long GetRandomNumberOfTicks(ISpecimenContext context)
    {
        return (long)_randomizer.Create(typeof(long), context);
    }
}