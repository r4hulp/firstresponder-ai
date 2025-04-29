export interface Transcript {
  durationMilliseconds: number
  combinedPhrases: CombinedPhrase[]
  phrases: Phrase[]
}

export interface CombinedPhrase {
  text: string
}

export interface Phrase {
  speaker: number
  offsetMilliseconds: number
  durationMilliseconds: number
  text: string
  words: Word[]
  locale: string
  confidence: number
}

export interface Word {
  text: string
  offsetMilliseconds: number
  durationMilliseconds: number
}
