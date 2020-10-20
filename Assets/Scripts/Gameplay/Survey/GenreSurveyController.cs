using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GenreSurveyController : MonoBehaviour
{
    public GenreEntry[] searchResults;
    public GenreEntry[] added;
    private string[] genresList = new string[]
	{
		"Electronic",
		"Dance House",
		"Tech Dance",
		"Vaporwave",
		"Meme",
		"Hardcore",
		"Jazz",
		"Deep House",
		"Electro House",
		"Hard Dance",
		"Hip-hop/Rap",
		"Pop",
		"K-Pop",
		"J-Pop",
		"Traditional",
		"Walt",
		"Hardstyle",
		"Gabber",
		"Ambient",
		"Future Funk",
		"City Pop",
		"Classical",
		"Rock",
		"Alternative",
		"Blue/R&B",
		"Country",
		"Dance",
		"Folk",
		"House",
		"Industrial",
		"Techno",
		"Trance",
		"Psy Trance",
		"French House",
		"Electro Pop",
		"Synth Pop",
		"Nu Disco",
		"Acid Trance",
		"Minimal",
		"Speedcore",
		"UK Hardcore",
		"Dubstep",
		"2 Step",
		"UK Garage",
		"Future House",
		"Happy Hardcore",
		"Big Beat",
		"Drum & Bass",
		"Tech Trance",
		"Tech House",
		"Eurobeat",
		"Rave",
		"Raggae",
		"Moombahton",
		"Big Room House"
	};

	public List<string> addedGenres = new List<string>();
	private SurveyPagesController surveyCtrl;

    private void OnEnable()
    {
		surveyCtrl = GetComponentInParent<SurveyPagesController>();
		UpdateGenreList();
	}

    public void SearchGenres(string query)
    {
		ShowAutoComplete(query, genresList, 7);

	}

	public void AddGenre(string value)
    {
		if(!addedGenres.Contains(value) && addedGenres.Count < 10)
        {
			addedGenres.Add(value);

		}
		UpdateGenreList();

	}

	public void RemoveGenre(string value)
	{
		if (addedGenres.Contains(value))
		{
			addedGenres.Remove(value);

		}
		UpdateGenreList();
	}

	public void UpdateGenreList()
    {
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < added.Length; i++)
		{
			if (i < addedGenres.Count)
			{
				added[i].gameObject.SetActive(true);
				added[i].updateText(addedGenres[i]);
				sb.Append(addedGenres[i]);
				sb.Append(", ");
			}
			else
			{
				added[i].gameObject.SetActive(false);
			}
		}

		surveyCtrl.updateAnswer(2, sb.ToString());

		if (addedGenres.Count > 0)
		{
			surveyCtrl.enableNextButton();
			
		}
		else
		{
			surveyCtrl.disableNextButton();
		}
	}


	#region Text AutoComplete
	private List<string> m_CacheCheckList = null;
	private string m_AutoCompleteLastInput;
	/// <summary>A textField to popup a matching popup, based on developers input values.</summary>
	/// <param name="input">string input.</param>
	/// <param name="source">the data of all possible values (string).</param>
	/// <param name="maxShownCount">the amount to display result.</param>
	/// <param name="levenshteinDistance">
	/// value between 0f ~ 1f,
	/// - more then 0f will enable the fuzzy matching
	/// - 1f = anything thing is okay.
	/// - 0f = require full match to the reference
	/// - recommend 0.4f ~ 0.7f
	/// </param>
	public void ShowAutoComplete(string input, string[] source, int maxShownCount = 5, float levenshteinDistance = 0.5f)
	{
		if (input.Length > 0)
		{
			if (m_AutoCompleteLastInput != input) // another field.
			{
				// Update cache
				m_AutoCompleteLastInput = input;

				List<string> uniqueSrc = new List<string>(new HashSet<string>(source)); // remove duplicate
				int srcCnt = uniqueSrc.Count;
				m_CacheCheckList = new List<string>(System.Math.Min(maxShownCount, srcCnt)); // optimize memory alloc

				// Start with - slow
				for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
				{
					if (uniqueSrc[i].ToLower().StartsWith(input.ToLower()))
					{
						
						m_CacheCheckList.Add(uniqueSrc[i]);
						uniqueSrc.RemoveAt(i);
						srcCnt--;
						i--;
					}
				}

				// Contains - very slow
				if (m_CacheCheckList.Count < 2)
				{
					for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
					{
						if (uniqueSrc[i].ToLower().Contains(input.ToLower()))
						{
							m_CacheCheckList.Add(uniqueSrc[i]);
							uniqueSrc.RemoveAt(i);
							srcCnt--;
							i--;
						}
					}
				}

				// Levenshtein Distance - very very slow.
				if (levenshteinDistance > 0f && // only developer request
					input.Length > 3 && // 3 characters on input, hidden value to avoid doing too early.
					m_CacheCheckList.Count < maxShownCount) // have some empty space for matching.
				{
					levenshteinDistance = Mathf.Clamp01(levenshteinDistance);
					string keywords = input.ToLower();
					for (int i = 0; i < srcCnt && m_CacheCheckList.Count < maxShownCount; i++)
					{
						int distance = LevenshteinDistance(uniqueSrc[i], keywords, caseSensitive: false);
						bool closeEnough = (int)(levenshteinDistance * uniqueSrc[i].Length) > distance;
						if (closeEnough)
						{
							m_CacheCheckList.Add(uniqueSrc[i]);
							uniqueSrc.RemoveAt(i);
							srcCnt--;
							i--;
						}
					}
				}
			}

			// Draw recommend keyward(s)
			m_CacheCheckList.Add(input);
			for (int i = 0; i < searchResults.Length; i++)
			{
				if (i < m_CacheCheckList.Count)
				{
					searchResults[i].gameObject.SetActive(true);
					searchResults[i].updateText(m_CacheCheckList[i]);
				}
				else
                {
					searchResults[i].gameObject.SetActive(false);
				}
			}
		}
	}

	/// <summary>Computes the Levenshtein Edit Distance between two enumerables.</summary>
	/// <typeparam name="T">The type of the items in the enumerables.</typeparam>
	/// <param name="lhs">The first enumerable.</param>
	/// <param name="rhs">The second enumerable.</param>
	/// <returns>The edit distance.</returns>
	/// <see cref="https://blogs.msdn.microsoft.com/toub/2006/05/05/generic-levenshtein-edit-distance-with-c/"/>
	public int LevenshteinDistance<T>(IEnumerable<T> lhs, IEnumerable<T> rhs) where T : System.IEquatable<T>
	{
		// Validate parameters
		if (lhs == null) throw new System.ArgumentNullException("lhs");
		if (rhs == null) throw new System.ArgumentNullException("rhs");

		// Convert the parameters into IList instances
		// in order to obtain indexing capabilities
		IList<T> first = lhs as IList<T> ?? new List<T>(lhs);
		IList<T> second = rhs as IList<T> ?? new List<T>(rhs);

		// Get the length of both.  If either is 0, return
		// the length of the other, since that number of insertions
		// would be required.
		int n = first.Count, m = second.Count;
		if (n == 0) return m;
		if (m == 0) return n;

		// Rather than maintain an entire matrix (which would require O(n*m) space),
		// just store the current row and the next row, each of which has a length m+1,
		// so just O(m) space. Initialize the current row.
		int curRow = 0, nextRow = 1;

		int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };
		for (int j = 0; j <= m; ++j)
			rows[curRow][j] = j;

		// For each virtual row (since we only have physical storage for two)
		for (int i = 1; i <= n; ++i)
		{
			// Fill in the values in the row
			rows[nextRow][0] = i;

			for (int j = 1; j <= m; ++j)
			{
				int dist1 = rows[curRow][j] + 1;
				int dist2 = rows[nextRow][j - 1] + 1;
				int dist3 = rows[curRow][j - 1] +
					(first[i - 1].Equals(second[j - 1]) ? 0 : 1);

				rows[nextRow][j] = System.Math.Min(dist1, System.Math.Min(dist2, dist3));
			}

			// Swap the current and next rows
			if (curRow == 0)
			{
				curRow = 1;
				nextRow = 0;
			}
			else
			{
				curRow = 0;
				nextRow = 1;
			}
		}

		// Return the computed edit distance
		return rows[curRow][m];
	}

	/// <summary>Computes the Levenshtein Edit Distance between two enumerables.</summary>
	/// <param name="lhs">The first enumerable.</param>
	/// <param name="rhs">The second enumerable.</param>
	/// <returns>The edit distance.</returns>
	/// <see cref="https://en.wikipedia.org/wiki/Levenshtein_distance"/>
	public int LevenshteinDistance(string lhs, string rhs, bool caseSensitive = true)
	{
		if (!caseSensitive)
		{
			lhs = lhs.ToLower();
			rhs = rhs.ToLower();
		}
		char[] first = lhs.ToCharArray();
		char[] second = rhs.ToCharArray();
		return LevenshteinDistance<char>(first, second);
	}
	#endregion
}
