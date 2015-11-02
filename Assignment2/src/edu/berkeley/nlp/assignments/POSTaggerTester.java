package edu.berkeley.nlp.assignments;

import edu.berkeley.nlp.io.PennTreebankReader;
import edu.berkeley.nlp.ling.Tree;
import edu.berkeley.nlp.ling.Trees;
import edu.berkeley.nlp.util.*;

import java.util.*;

/**
 * @author Dan Klein
 */
public class POSTaggerTester {

  static final String START_WORD = "<S>";
  static final String STOP_WORD = "</S>";
  static final String START_TAG = "<S>";
  static final String STOP_TAG = "</S>";

  /**
   * Tagged sentences are a bundling of a list of words and a list of their
   * tags.
   */
  static class TaggedSentence {
    final List<String> words;
    final List<String> tags;

    public int size() {
      return words.size();
    }

    public List<String> getWords() {
      return words;
    }

    public List<String> getTags() {
      return tags;
    }

    public String toString() {
      StringBuilder sb = new StringBuilder();
      for (int position = 0; position < words.size(); position++) {
        String word = words.get(position);
        String tag = tags.get(position);
        sb.append(word);
        sb.append("_");
        sb.append(tag);
      }
      return sb.toString();
    }

    public boolean equals(Object o) {
      if (this == o) return true;
      if (!(o instanceof TaggedSentence)) return false;

      final TaggedSentence taggedSentence = (TaggedSentence) o;

      if (tags != null ? !tags.equals(taggedSentence.tags) : taggedSentence.tags != null) return false;
      //noinspection RedundantIfStatement
      if (words != null ? !words.equals(taggedSentence.words) : taggedSentence.words != null) return false;

      return true;
    }

    public int hashCode() {
      int result;
      result = (words != null ? words.hashCode() : 0);
      result = 29 * result + (tags != null ? tags.hashCode() : 0);
      return result;
    }

    public TaggedSentence(List<String> words, List<String> tags) {
      this.words = words;
      this.tags = tags;
    }
  }

  /**
   * States are pairs of tags along with a position index, representing the two
   * tags preceding that position.  So, the START state, which can be gotten by
   * State.getStartState() is [START, START, 0].  To build an arbitrary state,
   * for example [DT, NN, 2], use the static factory method
   * State.buildState("DT", "NN", 2).  There isnt' a single final state, since
   * sentences lengths vary, so State.getEndState(i) takes a parameter for the
   * length of the sentence.
   */
  static class State {

    private static final transient Interner<State> stateInterner = new Interner<>(new Interner.CanonicalFactory<State>() {
      public State build(State state) {
        return new State(state);
      }
    });

    private static final transient State tempState = new State();

    public static State getStartState() {
      return buildState(START_TAG, START_TAG, 0);
    }

    public static State getStopState(int position) {
      return buildState(STOP_TAG, STOP_TAG, position);
    }

    public static State buildState(String previousPreviousTag, String previousTag, int position) {
      tempState.setState(previousPreviousTag, previousTag, position);
      return stateInterner.intern(tempState);
    }

    public static List<String> toTagList(List<State> states) {
      List<String> tags = new ArrayList<>();
      if (states.size() > 0) {
        tags.add(states.get(0).getPreviousPreviousTag());
        for (State state : states) {
          tags.add(state.getPreviousTag());
        }
      }
      return tags;
    }

    public int getPosition() {
      return position;
    }

    public String getPreviousTag() {
      return previousTag;
    }

    public String getPreviousPreviousTag() {
      return previousPreviousTag;
    }

    public State getNextState(String tag) {
      return State.buildState(getPreviousTag(), tag, getPosition() + 1);
    }

    public State getPreviousState(String tag) {
      return State.buildState(tag, getPreviousPreviousTag(), getPosition() - 1);
    }

    public boolean equals(Object o) {
      if (this == o) return true;
      if (!(o instanceof State)) return false;

      final State state = (State) o;

      if (position != state.position) return false;
      if (previousPreviousTag != null ? !previousPreviousTag.equals(state.previousPreviousTag) : state.previousPreviousTag != null) return false;
      //noinspection RedundantIfStatement
      if (previousTag != null ? !previousTag.equals(state.previousTag) : state.previousTag != null) return false;

      return true;
    }

    public int hashCode() {
      int result;
      result = position;
      result = 29 * result + (previousTag != null ? previousTag.hashCode() : 0);
      result = 29 * result + (previousPreviousTag != null ? previousPreviousTag.hashCode() : 0);
      return result;
    }

    public String toString() {
      return "[" + getPreviousPreviousTag() + ", " + getPreviousTag() + ", " + getPosition() + "]";
    }

    int position;
    String previousTag;
    String previousPreviousTag;

    private void setState(String previousPreviousTag, String previousTag, int position) {
      this.previousPreviousTag = previousPreviousTag;
      this.previousTag = previousTag;
      this.position = position;
    }

    private State() {
    }

    private State(State state) {
      setState(state.getPreviousPreviousTag(), state.getPreviousTag(), state.getPosition());
    }
  }

  /**
   * A Trellis is a graph with a start state an an end state, along with
   * successor and predecessor functions.
   */
  static class Trellis <S> {
    S startState;
    S endState;
    final CounterMap<S, S> forwardTransitions;
    final CounterMap<S, S> backwardTransitions;

    /**
     * Get the unique start state for this trellis.
     */
    public S getStartState() {
      return startState;
    }

    public void setStartState(S startState) {
      this.startState = startState;
    }

    /**
     * Get the unique end state for this trellis.
     */
    public S getEndState() {
      return endState;
    }

    public void setStopState(S endState) {
      this.endState = endState;
    }

    /**
     * For a given state, returns a counter over what states can be next in the
     * markov process, along with the cost of that transition.  Caution: a state
     * not in the counter is illegal, and should be considered to have cost
     * Double.NEGATIVE_INFINITY, but Counters score items they don't contain as
     * 0.
     */
    public Counter<S> getForwardTransitions(S state) {
      return forwardTransitions.getCounter(state);

    }


    /**
     * For a given state, returns a counter over what states can precede it in
     * the markov process, along with the cost of that transition.
     */
    public Counter<S> getBackwardTransitions(S state) {
      return backwardTransitions.getCounter(state);
    }

    public void setTransitionCount(S start, S end, double count) {
      forwardTransitions.setCount(start, end, count);
      backwardTransitions.setCount(end, start, count);
    }

    public Trellis() {
      forwardTransitions = new CounterMap<>();
      backwardTransitions = new CounterMap<>();
    }
  }

  /**
   * A TrellisDecoder takes a Trellis and returns a path through that trellis in
   * which the first item is trellis.getStartState(), the last is
   * trellis.getEndState(), and each pair of states is conntected in the
   * trellis.
   */
  interface TrellisDecoder <S> {
    List<S> getBestPath(Trellis<S> trellis);
  }

  static class GreedyDecoder <S> implements TrellisDecoder<S> {
    public List<S> getBestPath(Trellis<S> trellis) {
      List<S> states = new ArrayList<>();
      S currentState = trellis.getStartState();
      states.add(currentState);
      while (!currentState.equals(trellis.getEndState())) {
        Counter<S> transitions = trellis.getForwardTransitions(currentState);
        S nextState = transitions.argMax();
        states.add(nextState);
        currentState = nextState;
      }
      return states;
    }
  }

  static class ViterbiDecoder <S> implements TrellisDecoder<S> {
    CounterMap<Integer, S> piCache = new CounterMap<>();
    Map<Integer, CounterMap<S, S>> bpCache = new HashMap<>();
    Map<Integer, Set<S>> transitionQueue = new HashMap<>();

    public List<S> getBestPath(Trellis<S> trellis) {
      List<S> states = new ArrayList<>();
      piCache = new CounterMap<>();
      bpCache = new HashMap<>();
      S currentState = trellis.getStartState();
      piCache.setCount(0, currentState, 0.0);
      int pos = 0;
      transitionQueue.put(pos, new HashSet<S>());
      transitionQueue.get(pos).add(currentState);
      while (true) {
        if (!transitionQueue.containsKey(pos) || currentState.equals(trellis.getEndState()))
          break;

        for (S state : transitionQueue.get(pos)) {
          currentState = state;
          if (currentState.equals(trellis.getEndState()))
            break;

          Counter<S> forwardTransitions = trellis.getForwardTransitions(currentState);
          for (S nextState : forwardTransitions.keySet()) {
            GetPi(pos + 1, nextState, trellis);
            if (!transitionQueue.containsKey(pos + 1)) {
              transitionQueue.put(pos + 1, new HashSet<S>());
            }
            transitionQueue.get(pos + 1).add(nextState);
          }
        }

        pos++;
      }

      states.add(currentState);
      currentState = bpCache.get(pos - 1).getCounter(currentState).argMax();
      states.add(currentState);
      for (int i = pos - 2; i > 0; i--)
      {
        states.add(0, bpCache.get(i).getCounter(currentState).argMax());
        currentState = states.get(0);
      }

      return states;
    }

    public double GetPi(int pos, S state, Trellis<S> trellis)
    {
      if (piCache.containsKey(pos))
      {
        if (piCache.getCounter(pos).containsKey(state))
          return piCache.getCount(pos, state);
      }

      double max = Double.NEGATIVE_INFINITY;
      S maxPreviousState = null;
      Counter<S> backwardTransitions = trellis.getBackwardTransitions(state);
      for (S previousState : backwardTransitions.keySet())
      {
        double previousPi = GetPi(pos - 1, previousState, trellis);
        double previousProb = backwardTransitions.getCount(previousState);
        double previousStateProb = previousPi + previousProb;
        if (previousStateProb > max) {
          max = previousStateProb;
          maxPreviousState = previousState;
        }
      }

      piCache.setCount(pos, state, max);
      if (!bpCache.containsKey(pos))
      {
        bpCache.put(pos, new CounterMap<S, S>());
      }
      bpCache.get(pos).setCount(state, maxPreviousState, max);

      return max;
    }
  }

  static class POSTagger {

    final LocalTrigramScorer localTrigramScorer;
    final TrellisDecoder<State> trellisDecoder;

    // chop up the training instances into local contexts and pass them on to the local scorer.
    public void train(List<TaggedSentence> taggedSentences) {
      localTrigramScorer.train(extractLabeledLocalTrigramContexts(taggedSentences));
    }

    // chop up the validation instances into local contexts and pass them on to the local scorer.
    public void validate(List<TaggedSentence> taggedSentences) {
      localTrigramScorer.validate(extractLabeledLocalTrigramContexts(taggedSentences));
    }

    private List<LabeledLocalTrigramContext> extractLabeledLocalTrigramContexts(List<TaggedSentence> taggedSentences) {
      List<LabeledLocalTrigramContext> localTrigramContexts = new ArrayList<>();
      for (TaggedSentence taggedSentence : taggedSentences) {
        localTrigramContexts.addAll(extractLabeledLocalTrigramContexts(taggedSentence));
      }
      return localTrigramContexts;
    }

    private List<LabeledLocalTrigramContext> extractLabeledLocalTrigramContexts(TaggedSentence taggedSentence) {
      List<LabeledLocalTrigramContext> labeledLocalTrigramContexts = new ArrayList<>();
      List<String> words = new BoundedList<>(taggedSentence.getWords(), START_WORD, STOP_WORD);
      List<String> tags = new BoundedList<>(taggedSentence.getTags(), START_TAG, STOP_TAG);
      for (int position = 0; position <= taggedSentence.size() + 1; position++) {
        labeledLocalTrigramContexts.add(new LabeledLocalTrigramContext(words, position, tags.get(position - 2), tags.get(position - 1), tags.get(position)));
      }
      return labeledLocalTrigramContexts;
    }

    /**
     * Builds a Trellis over a sentence, by starting at the state State, and
     * advancing through all legal extensions of each state already in the
     * trellis.  You should not have to modify this code (or even read it,
     * really).
     */
    private Trellis<State> buildTrellis(List<String> sentence) {
      Trellis<State> trellis = new Trellis<>();
      trellis.setStartState(State.getStartState());
      State stopState = State.getStopState(sentence.size() + 2);
      trellis.setStopState(stopState);
      Set<State> states = Collections.singleton(State.getStartState());
      for (int position = 0; position <= sentence.size() + 1; position++) {
        Set<State> nextStates = new HashSet<>();
        for (State state : states) {
          if (state.equals(stopState))
            continue;
          LocalTrigramContext localTrigramContext = new LocalTrigramContext(sentence, position, state.getPreviousPreviousTag(), state.getPreviousTag());
          Counter<String> tagScores = localTrigramScorer.getLogScoreCounter(localTrigramContext);
          for (String tag : tagScores.keySet()) {
            double score = tagScores.getCount(tag);
            State nextState = state.getNextState(tag);
            trellis.setTransitionCount(state, nextState, score);
            nextStates.add(nextState);
          }
        }
//        System.out.println("States: "+nextStates);
        states = nextStates;
      }
      return trellis;
    }

    // to tag a sentence: build its trellis and find a path through that trellis
    public List<String> tag(List<String> sentence) {
      Trellis<State> trellis = buildTrellis(sentence);
      List<State> states = trellisDecoder.getBestPath(trellis);
      List<String> tags = State.toTagList(states);
      tags = stripBoundaryTags(tags);
      return tags;
    }

    /**
     * Scores a tagging for a sentence.  Note that a tag sequence not accepted
     * by the markov process should receive a log score of
     * Double.NEGATIVE_INFINITY.
     */
    public double scoreTagging(TaggedSentence taggedSentence) {
      double logScore = 0.0;
      List<LabeledLocalTrigramContext> labeledLocalTrigramContexts = extractLabeledLocalTrigramContexts(taggedSentence);
      for (LabeledLocalTrigramContext labeledLocalTrigramContext : labeledLocalTrigramContexts) {
        Counter<String> logScoreCounter = localTrigramScorer.getLogScoreCounter(labeledLocalTrigramContext);
        String currentTag = labeledLocalTrigramContext.getCurrentTag();
        if (logScoreCounter.containsKey(currentTag)) {
          logScore += logScoreCounter.getCount(currentTag);
        } else {
          logScore += Double.NEGATIVE_INFINITY;
        }
      }
      return logScore;
    }

    private List<String> stripBoundaryTags(List<String> tags) {
      return tags.subList(2, tags.size() - 2);
    }

    public POSTagger(LocalTrigramScorer localTrigramScorer, TrellisDecoder<State> trellisDecoder) {
      this.localTrigramScorer = localTrigramScorer;
      this.trellisDecoder = trellisDecoder;
    }
  }

  /**
   * A LocalTrigramContext is a position in a sentence, along with the previous
   * two tags -- basically a FeatureVector.
   */
  static class LocalTrigramContext {
    final List<String> words;
    final int position;
    final String previousTag;
    final String previousPreviousTag;

    public List<String> getWords() {
      return words;
    }

    public String getCurrentWord() {
      return words.get(position);
    }

    public int getPosition() {
      return position;
    }

    public String getPreviousTag() {
      return previousTag;
    }

    public String getPreviousPreviousTag() {
      return previousPreviousTag;
    }

    public String toString() {
      return "[" + getPreviousPreviousTag() + ", " + getPreviousTag() + ", " + getCurrentWord() + "]";
    }

    public LocalTrigramContext(List<String> words, int position, String previousPreviousTag, String previousTag) {
      this.words = words;
      this.position = position;
      this.previousTag = previousTag;
      this.previousPreviousTag = previousPreviousTag;
    }
  }

  /**
   * A LabeledLocalTrigramContext is a context plus the correct tag for that
   * position -- basically a LabeledFeatureVector
   */
  static class LabeledLocalTrigramContext extends LocalTrigramContext {
    final String currentTag;

    public String getCurrentTag() {
      return currentTag;
    }

    public String toString() {
      return "[" + getPreviousPreviousTag() + ", " + getPreviousTag() + ", " + getCurrentWord() + "_" + getCurrentTag() + "]";
    }

    public LabeledLocalTrigramContext(List<String> words, int position, String previousPreviousTag, String previousTag, String currentTag) {
      super(words, position, previousPreviousTag, previousTag);
      this.currentTag = currentTag;
    }
  }

  /**
   * LocalTrigramScorers assign scores to tags occuring in specific
   * LocalTrigramContexts.
   */
  public interface LocalTrigramScorer {
    /**
     * The Counter returned should contain log probabilities, meaning if all
     * values are exponentiated and summed, they should sum to one (if it's a 
     * single conditional pobability). For efficiency, the Counter can
     * contain only the tags which occur in the given context
     * with non-zero model probability.
     */
    Counter<String> getLogScoreCounter(LocalTrigramContext localTrigramContext);

    void train(List<LabeledLocalTrigramContext> localTrigramContexts);

    void validate(List<LabeledLocalTrigramContext> localTrigramContexts);
  }

  /**
   * The MostFrequentTagScorer gives each test word the tag it was seen with
   * most often in training (or the tag with the most seen word types if the
   * test word is unseen in training.  This scorer actually does a little more
   * than its name claims -- if constructed with restrictTrigrams = true, it
   * will forbid illegal tag trigrams, otherwise it makes no use of tag history
   * information whatsoever.
   */
  static class MostFrequentTagScorer implements LocalTrigramScorer {

    final boolean restrictTrigrams; // if true, assign log score of Double.NEGATIVE_INFINITY to illegal tag trigrams.

    CounterMap<String, String> wordsToTags = new CounterMap<>();
    Counter<String> unknownWordTags = new Counter<>();
    final Set<String> seenTagTrigrams = new HashSet<>();

    public int getHistorySize() {
      return 2;
    }

    public Counter<String> getLogScoreCounter(LocalTrigramContext localTrigramContext) {
      int position = localTrigramContext.getPosition();
      String word = localTrigramContext.getWords().get(position);
      Counter<String> tagCounter = unknownWordTags;
      if (wordsToTags.keySet().contains(word)) {
        tagCounter = wordsToTags.getCounter(word);
      }
      Set<String> allowedFollowingTags = allowedFollowingTags(tagCounter.keySet(), localTrigramContext.getPreviousPreviousTag(), localTrigramContext.getPreviousTag());
      Counter<String> logScoreCounter = new Counter<>();
      for (String tag : tagCounter.keySet()) {
        double logScore = Math.log(tagCounter.getCount(tag));
        if (!restrictTrigrams || allowedFollowingTags.isEmpty() || allowedFollowingTags.contains(tag))
          logScoreCounter.setCount(tag, logScore);
      }
      return logScoreCounter;
    }

    private Set<String> allowedFollowingTags(Set<String> tags, String previousPreviousTag, String previousTag) {
      Set<String> allowedTags = new HashSet<>();
      for (String tag : tags) {
        String trigramString = makeTrigramString(previousPreviousTag, previousTag, tag);
        if (seenTagTrigrams.contains((trigramString))) {
          allowedTags.add(tag);
        }
      }
      return allowedTags;
    }

    private String makeTrigramString(String previousPreviousTag, String previousTag, String currentTag) {
      StringBuilder sb = new StringBuilder(previousPreviousTag.length() + previousTag.length() + currentTag.length() + 2);
      sb.append(previousPreviousTag);
      sb.append(" ");
      sb.append(previousTag);
      sb.append(" ");
      sb.append(currentTag);

      return sb.toString();
    }

    public void train(List<LabeledLocalTrigramContext> labeledLocalTrigramContexts) {
      // collect word-tag counts
      for (LabeledLocalTrigramContext labeledLocalTrigramContext : labeledLocalTrigramContexts) {
        String word = labeledLocalTrigramContext.getCurrentWord();
        String tag = labeledLocalTrigramContext.getCurrentTag();
        if (!wordsToTags.keySet().contains(word)) {
          // word is currently unknown, so tally its tag in the unknown tag counter
          unknownWordTags.incrementCount(tag, 1.0);
        }
        wordsToTags.incrementCount(word, tag, 1.0);
        seenTagTrigrams.add(makeTrigramString(labeledLocalTrigramContext.getPreviousPreviousTag(), labeledLocalTrigramContext.getPreviousTag(), labeledLocalTrigramContext.getCurrentTag()));
      }
      wordsToTags = Counters.conditionalNormalize(wordsToTags);
      unknownWordTags = Counters.normalize(unknownWordTags);
    }

    public void validate(List<LabeledLocalTrigramContext> labeledLocalTrigramContexts) {
      // no tuning for this dummy model!
    }

    public MostFrequentTagScorer(boolean restrictTrigrams) {
      this.restrictTrigrams = restrictTrigrams;
    }
  }

  static class HMMTagScorer implements LocalTrigramScorer {

    double trigramTagLambda;
    double bigramTagLambda;
    double unigramTagLambda;
    int unknownCountCutOff;

    CounterMap<String, String> wordsToTags = new CounterMap<>();
    Counter<String> trigramTags = new Counter<>();
    Counter<String> bigramTags = new Counter<>();
    Counter<String> tags = new Counter<>();
    Counter<String> unknownWordTags = new Counter<>();
    Counter<String> unknownNumber = new Counter<>();
    Counter<String> unknownSymbol = new Counter<>();
    Counter<String> unknownAlphaAndSymbol = new Counter<>();
    Counter<String> unknownDigitAndSymbol = new Counter<>();
    Counter<String> unknownAllLetters = new Counter<>();
    Counter<String> unknownLowerCase = new Counter<>();
    Counter<String> unknownAlphaAndAnd = new Counter<>();
    Counter<String> unknownAlphaAndSymbolAndDash = new Counter<>();
    Counter<String> unknownAlphaAndDash = new Counter<>();
    Counter<String> unknownApostropheWord = new Counter<>();
    Counter<String> unknownApostropheAlpha = new Counter<>();
    Counter<String> unknownNegation = new Counter<>();
    Counter<String> unknownTwoDigitNumTags = new Counter<>();
    Counter<String> unknownFourDigitNumTags = new Counter<>();
    Counter<String> unknownDigitAndAlpha = new Counter<>();
    Counter<String> unknownDigitAndDash = new Counter<>();
    Counter<String> unknownDigitAndAlphaAndSymbolDash = new Counter<>();
    Counter<String> unknownDigitAndSlash = new Counter<>();
    Counter<String> unknownDigitAndForwardSlash = new Counter<>();
    Counter<String> unknownDigitAndComma = new Counter<>();
    Counter<String> unknownDigitAndCommaAndPeriod = new Counter<>();
    Counter<String> unknownDigitAndPeriod = new Counter<>();
    Counter<String> unknownWordAllCaps = new Counter<>();
    Counter<String> unknownCapAndPeriod = new Counter<>();
    Counter<String> unknownAbbreviation = new Counter<>();
    Counter<String> unknownInitCap = new Counter<>();
    Counter<String> unknownFirstWord = new Counter<>();
    Counter<String> unknownWords = new Counter<>();
    double totalTags;
    double totalWords = 19; // Account for the unknown types.
    final Set<String> seenTagTrigrams = new HashSet<>();
    Set<String> seenTags = new HashSet<>();
    Set<String> seenWords = new HashSet<>();


    public int getHistorySize() {
      return 2;
    }

    public Counter<String> getLogScoreCounter(LocalTrigramContext localTrigramContext) {
      int position = localTrigramContext.getPosition();
      String word = localTrigramContext.getWords().get(position);
      Counter<String> tagCounter = GetUnknownTypeCounter(word, localTrigramContext.getPosition());
      if (wordsToTags.keySet().contains(word)) {
        tagCounter = wordsToTags.getCounter(word);
      }
      Counter<String> logScoreCounter = new Counter<>();
      for (String tag : tagCounter.keySet()) {
        String currentTrigram = makeTrigramString(localTrigramContext.getPreviousPreviousTag(), localTrigramContext.getPreviousTag(), tag);
        String currentBigram = makeBigramString(localTrigramContext.getPreviousTag(), tag);
        double trigramCount = trigramTags.getCount(currentTrigram);
        double bigramCount = bigramTags.getCount(currentBigram);
        double unigramCount = tags.getCount(tag);
        double trigramProbability = bigramCount == 0 ? 0 : trigramTagLambda *(trigramCount / bigramCount);
        double bigramProbability = unigramCount == 0 ? 0 : bigramTagLambda *(bigramCount / unigramCount);
        double unigramProbability = unigramTagLambda *(unigramCount / totalTags);
        double tagProbability = trigramProbability + bigramProbability + unigramProbability;

        double emissionProbability = tagCounter.getCount(tag);
        double logScore = Math.log(tagProbability * emissionProbability);
        logScoreCounter.setCount(tag, logScore);
      }
      return logScoreCounter;
    }

    private String makeTrigramString(String previousPreviousTag, String previousTag, String currentTag) {
      StringBuilder sb = new StringBuilder(previousPreviousTag.length() + previousTag.length() + currentTag.length() + 2);
      sb.append(previousPreviousTag);
      sb.append(" ");
      sb.append(previousTag);
      sb.append(" ");
      sb.append(currentTag);

      return sb.toString();
    }

    private String makeBigramString(String previousTag, String currentTag) {
      StringBuilder sb = new StringBuilder(previousTag.length() + currentTag.length() + 1);
      sb.append(previousTag);
      sb.append(" ");
      sb.append(currentTag);

      return sb.toString();
    }

    private Counter<String> GetUnknownTypeCounter(String word, int pos)
    {
      if (pos == 0)
        return unknownFirstWord;
      else if (word.matches("^[^a-zA-Z0-9]+$"))
        return unknownSymbol;
      else if (word.matches("^'[a-zA-Z]+$"))
        return unknownApostropheWord;
      else if (word.matches("^[a-zA-Z]'[a-zA-Z]$"))
        return unknownNegation;
      else if (word.matches("^[a-zA-Z]+'[a-zA-Z]+$"))
        return unknownApostropheAlpha;
      else if (word.matches("^[A-Z][a-z]+$"))
        return unknownInitCap;
      else if (word.matches("^[A-Z]\\.$"))
        return unknownCapAndPeriod;
      else if (word.matches("^[a-zA-Z]{1,5}\\.([a-zA-Z]{1,1}(\\.([a-zA-Z]{1,1}(\\.([a-zA-Z]{1,1}(\\.([a-zA-Z]{1,1}(\\.)?)?)?)?)?)?)?)?$"))
        return unknownAbbreviation;
      else if (word.matches("^[A-Z]+$"))
        return unknownWordAllCaps;
      else if (word.matches("^[a-z]+$"))
        return unknownLowerCase;
      else if (word.matches("[a-zA-Z]+$"))
        return unknownAllLetters;
      else if (word.matches("[a-zA-Z\\-]+$"))
        return unknownAlphaAndDash;
      else if (word.matches("[a-zA-Z&]+$"))
        return unknownAlphaAndAnd;
      else if (word.matches("^[0-9]*\\.[0-9]+$"))
        return unknownDigitAndPeriod;
      else if (word.matches("^[0-9]*,[0-9]+$"))
        return unknownDigitAndComma;
      else if (word.matches("^[0-9]+,[0-9]+\\.[0-9]+$"))
        return unknownDigitAndCommaAndPeriod;
      else if (word.matches("^[0-9]{1,2}$"))
        return unknownTwoDigitNumTags;
      else if (word.matches("^[0-9]{1,4}$"))
        return unknownFourDigitNumTags;
      else if (word.matches("^[0-9]+$"))
        return unknownNumber;
      else if (word.matches("^[0-9\\-]+$"))
        return unknownDigitAndDash;
      else if (word.matches("^[0-9\\\\]+$"))
        return unknownDigitAndSlash;
      else if (word.matches("^[0-9/]+$"))
        return unknownDigitAndForwardSlash;
      else if (word.matches("^[a-zA-Z0-9]+$"))
        return unknownDigitAndAlpha;
      else if (word.matches("^[a-zA-Z0-9\\-]+$"))
        return unknownAlphaAndSymbolAndDash;
      else if (word.matches("[a-zA-Z0-9\\.'\\-]+$"))
        return unknownDigitAndAlphaAndSymbolDash;
      else if (word.matches("^[^0-9]+$"))
        return unknownAlphaAndSymbol;
      else if (word.matches("^[^a-zA-Z]+$"))
        return unknownDigitAndSymbol;
      else
        return unknownWordTags;
    }

    public void train(List<LabeledLocalTrigramContext> labeledLocalTrigramContexts) {
      // collect word-tag counts
      for (LabeledLocalTrigramContext labeledLocalTrigramContext : labeledLocalTrigramContexts) {
        String word = labeledLocalTrigramContext.getCurrentWord();
        String tag = labeledLocalTrigramContext.getCurrentTag();
        if (!seenWords.contains(word) || unknownWords.getCount(word) < unknownCountCutOff) {
          // word is currently unknown, so tally its tag in the unknown tag counter
          Counter<String> unknownTypeCounter = GetUnknownTypeCounter(word, labeledLocalTrigramContext.getPosition());
          unknownTypeCounter.incrementCount(tag, 1.0);
          unknownWords.incrementCount(word, 1.0);
        }
        String trigramStringTags = makeTrigramString(labeledLocalTrigramContext.getPreviousPreviousTag(), labeledLocalTrigramContext.getPreviousTag(), labeledLocalTrigramContext.getCurrentTag());
        String bigramStringTags = makeBigramString(labeledLocalTrigramContext.getPreviousTag(), labeledLocalTrigramContext.getCurrentTag());

        wordsToTags.incrementCount(word, tag, 1.0);
        trigramTags.incrementCount(trigramStringTags, 1.0);
        bigramTags.incrementCount(bigramStringTags, 1.0);
        tags.incrementCount(tag, 1.0);
        seenTagTrigrams.add(trigramStringTags);
        seenTags.add(tag);
        seenWords.add(word);
        totalTags++;
        totalWords++;
      }
    }

    public void validate(List<LabeledLocalTrigramContext> labeledLocalTrigramContexts) {
      // no tuning for this dummy model!
    }

    public HMMTagScorer() {
      this(0.6, 0.25, 0.15, 5);
    }

    public HMMTagScorer(double trigramTagLambda, double bigramTagLambda, double unigramTagLambda, int unknownCountCutOff) {
      this.trigramTagLambda = trigramTagLambda;
      this.bigramTagLambda = bigramTagLambda;
      this.unigramTagLambda = unigramTagLambda;
      this.unknownCountCutOff = unknownCountCutOff;
    }
  }

  private static List<TaggedSentence> readTaggedSentences(String path, int low, int high) {
    Collection<Tree<String>> trees = PennTreebankReader.readTrees(path, low, high);
    List<TaggedSentence> taggedSentences = new ArrayList<>();
    Trees.TreeTransformer<String> treeTransformer = new Trees.EmptyNodeStripper();
    for (Tree<String> tree : trees) {
      tree = treeTransformer.transformTree(tree);
      List<String> words = new BoundedList<>(new ArrayList<>(tree.getYield()), START_WORD, STOP_WORD);
      List<String> tags = new BoundedList<>(new ArrayList<>(tree.getPreTerminalYield()), START_TAG, STOP_TAG);
      taggedSentences.add(new TaggedSentence(words, tags));
    }
    return taggedSentences;
  }

  private static void evaluateTagger(POSTagger posTagger, List<TaggedSentence> taggedSentences, Set<String> trainingVocabulary, boolean verbose) {
    double numTags = 0.0;
    double numTagsCorrect = 0.0;
    double numUnknownWords = 0.0;
    double numUnknownWordsCorrect = 0.0;
    int numDecodingInversions = 0;
    for (TaggedSentence taggedSentence : taggedSentences) {
      List<String> words = taggedSentence.getWords();
      List<String> goldTags = taggedSentence.getTags();
      List<String> guessedTags = posTagger.tag(words);
      for (int position = 0; position < words.size() - 1; position++) {
        String word = words.get(position);
        String goldTag = goldTags.get(position);
        String guessedTag = guessedTags.get(position);
        if (guessedTag.equals(goldTag))
          numTagsCorrect += 1.0;
        numTags += 1.0;
        if (!trainingVocabulary.contains(word)) {
          if (guessedTag.equals(goldTag))
            numUnknownWordsCorrect += 1.0;
          numUnknownWords += 1.0;
        }
      }
      double scoreOfGoldTagging = posTagger.scoreTagging(taggedSentence);
      double scoreOfGuessedTagging = posTagger.scoreTagging(new TaggedSentence(words, guessedTags));
      if (scoreOfGoldTagging > scoreOfGuessedTagging) {
        numDecodingInversions++;
        if (verbose) System.out.println("WARNING: Decoder suboptimality detected.  Gold tagging has higher score than guessed tagging.");
      }
      if (verbose) System.out.println(alignedTaggings(words, goldTags, guessedTags, true) + "\n");
    }
    System.out.println("Tag Accuracy: " + (numTagsCorrect / numTags) + " (Unknown Accuracy: " + (numUnknownWordsCorrect / numUnknownWords) + ")  Decoder Suboptimalities Detected: " + numDecodingInversions);
  }

  // pretty-print a pair of taggings for a sentence, possibly suppressing the tags which correctly match
  private static String alignedTaggings(List<String> words, List<String> goldTags, List<String> guessedTags, boolean suppressCorrectTags) {
    StringBuilder goldSB = new StringBuilder("Gold Tags: ");
    StringBuilder guessedSB = new StringBuilder("Guessed Tags: ");
    StringBuilder wordSB = new StringBuilder("Words: ");
    for (int position = 0; position < words.size(); position++) {
      equalizeLengths(wordSB, goldSB, guessedSB);
      String word = words.get(position);
      String gold = goldTags.get(position);
      String guessed = guessedTags.get(position);
      wordSB.append(word);
      if (position < words.size() - 1)
        wordSB.append(' ');
      boolean correct = (gold.equals(guessed));
      if (correct && suppressCorrectTags)
        continue;
      guessedSB.append(guessed);
      goldSB.append(gold);
    }
    return goldSB + "\n" + guessedSB + "\n" + wordSB;
  }

  private static void equalizeLengths(StringBuilder sb1, StringBuilder sb2, StringBuilder sb3) {
    int maxLength = sb1.length();
    maxLength = Math.max(maxLength, sb2.length());
    maxLength = Math.max(maxLength, sb3.length());
    ensureLength(sb1, maxLength);
    ensureLength(sb2, maxLength);
    ensureLength(sb3, maxLength);
  }

  private static void ensureLength(StringBuilder sb, int length) {
    while (sb.length() < length) {
      sb.append(' ');
    }
  }

  private static Set<String> extractVocabulary(List<TaggedSentence> taggedSentences) {
    Set<String> vocabulary = new HashSet<>();
    for (TaggedSentence taggedSentence : taggedSentences) {
      List<String> words = taggedSentence.getWords();
      vocabulary.addAll(words);
    }
    return vocabulary;
  }

  public static void main(String[] args) {
    // Parse command line flags and arguments
    Map<String, String> argMap = CommandLineUtils.simpleCommandLineParser(args);

    // Set up default parameters and settings
    String basePath = ".";
    boolean verbose = false;
    boolean useValidation = true;

    // Update defaults using command line specifications

    // The path to the assignment data
    if (argMap.containsKey("-path")) {
      basePath = argMap.get("-path");
    }
    System.out.println("Using base path: " + basePath);

    // Whether to use the validation or test set
    if (argMap.containsKey("-test")) {
      String testString = argMap.get("-test");
      if (testString.equalsIgnoreCase("test"))
        useValidation = false;
    }
    System.out.println("Testing on: " + (useValidation ? "validation" : "test"));

    // Whether or not to print the individual errors.
    if (argMap.containsKey("-verbose")) {
      verbose = true;
    }

    // Read in data
    System.out.print("Loading training sentences...");
    List<TaggedSentence> trainTaggedSentences = readTaggedSentences(basePath, 200, 2199);
    Set<String> trainingVocabulary = extractVocabulary(trainTaggedSentences);
    System.out.println("done.");
    System.out.print("Loading validation sentences...");
    List<TaggedSentence> validationTaggedSentences = readTaggedSentences(basePath, 2200, 2299);
    System.out.println("done.");
    System.out.print("Loading test sentences...");
    List<TaggedSentence> testTaggedSentences = readTaggedSentences(basePath, 2300, 2399);
    System.out.println("done.");

    // Construct tagger components
    // TODO : improve on the MostFrequentTagScorer
    //LocalTrigramScorer localTrigramScorer = new MostFrequentTagScorer(false);
    LocalTrigramScorer localTrigramScorer = new HMMTagScorer();
    // TODO : improve on the GreedyDecoder
    //TrellisDecoder<State> trellisDecoder = new GreedyDecoder<>();
    TrellisDecoder<State> trellisDecoder = new ViterbiDecoder<>();

    // Train tagger
    POSTagger posTagger = new POSTagger(localTrigramScorer, trellisDecoder);
    posTagger.train(trainTaggedSentences);
    posTagger.validate(validationTaggedSentences);

    // Evaluation set, use either test of validation (for dev)
    final List<TaggedSentence> evalTaggedSentences;
    if (useValidation) {
        evalTaggedSentences = validationTaggedSentences;
    } else {
        evalTaggedSentences = testTaggedSentences;
    }
    
    // Test tagger
    evaluateTagger(posTagger, evalTaggedSentences, trainingVocabulary, verbose);
  }
}
