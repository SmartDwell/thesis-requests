namespace Thesis.Requests.Contracts.Search;

/// <summary>
/// Инструментарий для алгоритма Джаро-Винклера
/// </summary>
public static class JaroWinklerDistance
{
	/// <summary>
	/// Лимит веса для алгоритма, значение из оригинального paper
	/// </summary>
	private static readonly double WeightThreshold = 0.7;

	/// <summary>
	/// Длина префикса
	/// </summary>
	private static readonly int PrefixLength = 4;

	/// <summary>
	/// Возвращает сходство Джаро-Винклера для двух строк, в промежутке от 0 до 1 <br />
	/// Поиск симметричен <br />
	/// 0 - нет совпадения <br />
	/// 1 - есть совпадения <br />
	/// https://stackoverflow.com/a/19165108/16029300
	/// </summary>
	/// <param name="origin">Первая строка</param>
	/// <param name="searchValue">Вторая строка</param>
	public static double Proximity(string origin, string searchValue, IEqualityComparer<char>? comparer = null)
	{
		comparer ??= EqualityComparer<char>.Default;

		var lLen1 = origin.Length;
		var lLen2 = searchValue.Length;

		if (lLen1 == 0)
			return lLen2 == 0 ? 1.0 : 0.0;

		var lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

		var lMatched1 = new bool[lLen1];
		var lMatched2 = new bool[lLen2];

		var lNumCommon = 0;
		for (var i = 0; i < lLen1; ++i)
		{
			var lStart = Math.Max(0, i - lSearchRange);
			var lEnd = Math.Min(i + lSearchRange + 1, lLen2);
			for (var j = lStart; j < lEnd; ++j)
			{
				if (lMatched2[j])
					continue;

				if (!comparer.Equals(origin[i], searchValue[j]))
					continue;

				lMatched1[i] = true;
				lMatched2[j] = true;
				++lNumCommon;
				break;
			}
		}

		if (lNumCommon == 0)
			return 0.0;

		var lNumHalfTransposed = 0;
		var k = 0;

		for (var i = 0; i < lLen1; ++i)
		{
			if (!lMatched1[i]) 
				continue;

			while (!lMatched2[k]) 
				++k;

			if (!comparer.Equals(origin[i], searchValue[k]))
				++lNumHalfTransposed;

			++k;
		}

		var lNumTransposed = lNumHalfTransposed / 2;

		double lNumCommonD = lNumCommon;

		var lWeight = (lNumCommonD / lLen1
						  + lNumCommonD / lLen2
						  + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

		if (lWeight <= WeightThreshold) 
			return lWeight;

		var lMax = Math.Min(PrefixLength, Math.Min(origin.Length, searchValue.Length));
		var lPos = 0;

		while (lPos < lMax && comparer.Equals(origin[lPos], searchValue[lPos]))
			++lPos;

		if (lPos == 0) 
			return lWeight;

		return lWeight + 0.1 * lPos * (1.0 - lWeight);
	}
}
