using System.Collections;

namespace PL;

internal class ActiveFilterCollection : IEnumerable
{
    static readonly IEnumerable<BO.ActiveFilter> s_enums= (Enum.GetValues(typeof(BO.ActiveFilter)) as IEnumerable<BO.ActiveFilter>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}


