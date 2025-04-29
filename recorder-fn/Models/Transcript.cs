using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recorder_fn.Models
{
	public class CombinedPhrase
	{
		public string text { get; set; }
	}

	public class Phrase
	{
		public int speaker { get; set; }
		public int offsetMilliseconds { get; set; }
		public int durationMilliseconds { get; set; }
		public string text { get; set; }
		public List<Word> words { get; set; }
		public string locale { get; set; }
		public double confidence { get; set; }
	}

	public class Transcript
	{
		public int durationMilliseconds { get; set; }
		public List<CombinedPhrase> combinedPhrases { get; set; }
		public List<Phrase> phrases { get; set; }
	}

	public class Word
	{
		public string text { get; set; }
		public int offsetMilliseconds { get; set; }
		public int durationMilliseconds { get; set; }
	}




}
