package edu.berkeley.nlp.assignments;

import edu.berkeley.nlp.util.CommandLineUtils;

import java.util.*;

/**
 * Created by azend on 11/2/2015.
 */
public class gridSearch {
    public static void main(String[] args)
    {
        Map<Double, ArrayList<String>> resultsTotal = new HashMap<>();
        Map<Double, ArrayList<String>> resultsUnknown = new HashMap<>();

        Map<String, String> argMap = CommandLineUtils.simpleCommandLineParser(args);
        double hmmTrigramLambda = 0.6;
        double hmmBigramLambda = 0.25;
        double hmmUnigramLambda = 0.15;
        int hmmUncommonWordsCutOff = 5;
        String hmmArguments;

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

        if (argMap.containsKey("-hmmArguments"))
        {
            hmmArguments = argMap.get("-hmmArguments");
            String[] arguments = hmmArguments.split(";");
            if (arguments.length < 4)
            {
                System.out.println("No enough Hmm arguments, ignoring input.");
            }
            else
            {
                hmmTrigramLambda = Double.parseDouble(arguments[0]);
                hmmBigramLambda = Double.parseDouble(arguments[1]);
                hmmUnigramLambda = Double.parseDouble(arguments[2]);
                hmmUncommonWordsCutOff = Integer.parseInt(arguments[3]);
            }
        }

        // Read in data
        System.out.print("Loading training sentences...");
        List<POSTaggerTester.TaggedSentence> trainTaggedSentences = POSTaggerTester.readTaggedSentences(basePath, 200, 2199);
        Set<String> trainingVocabulary = POSTaggerTester.extractVocabulary(trainTaggedSentences);
        System.out.println("done.");
        System.out.print("Loading validation sentences...");
        List<POSTaggerTester.TaggedSentence> validationTaggedSentences = POSTaggerTester.readTaggedSentences(basePath, 2200, 2299);
        System.out.println("done.");
        System.out.print("Loading test sentences...");
        List<POSTaggerTester.TaggedSentence> testTaggedSentences = POSTaggerTester.readTaggedSentences(basePath, 2300, 2399);
        System.out.println("done.");



        for (double i = 0.33; i < 1; i+=0.01)
        {
            for (double j = 0.01; j < 1; j+=0.01)
            {
                double k = 1 - (i + j);
                if (k <= 0)
                    break;

                for (int l = 3; l < 8; l++) {
                    StringBuilder sb = new StringBuilder(7);
                    sb.append(i);
                    sb.append(";");
                    sb.append(j);
                    sb.append(";");
                    sb.append(k);
                    sb.append(";");
                    sb.append(l);
                    System.out.println("Testing with "+ sb.toString());
                    // Construct tagger components
                    // TODO : improve on the MostFrequentTagScorer
                    //LocalTrigramScorer localTrigramScorer = new MostFrequentTagScorer(false);
                    POSTaggerTester.LocalTrigramScorer localTrigramScorer = new POSTaggerTester.HMMTagScorer(i, j, k, l);
                    // TODO : improve on the GreedyDecoder
                    //TrellisDecoder<State> trellisDecoder = new GreedyDecoder<>();
                    POSTaggerTester.TrellisDecoder<POSTaggerTester.State> trellisDecoder = new POSTaggerTester.ViterbiDecoder<>();

                    // Train tagger
                    POSTaggerTester.POSTagger posTagger = new POSTaggerTester.POSTagger(localTrigramScorer, trellisDecoder);
                    posTagger.train(trainTaggedSentences);
                    posTagger.validate(validationTaggedSentences);

                    // Evaluation set, use either test of validation (for dev)
                    final List<POSTaggerTester.TaggedSentence> evalTaggedSentences;
                    if (useValidation) {
                        evalTaggedSentences = validationTaggedSentences;
                    } else {
                        evalTaggedSentences = testTaggedSentences;
                    }

                    // Test tagger
                    POSTaggerTester.evaluateTagger(posTagger, evalTaggedSentences, trainingVocabulary, verbose);

                    double totalPercentage =  POSTaggerTester.GetLatestTotalPercentage();
                    double unknownPercentage = POSTaggerTester.GetLatestTotalPercentage();
                    if (!resultsTotal.containsKey(totalPercentage))
                    {
                        resultsTotal.put(totalPercentage, new ArrayList<String>());
                    }
                    if (!resultsUnknown.containsKey(unknownPercentage))
                    {
                        resultsUnknown.put(unknownPercentage, new ArrayList<String>());
                    }

                    resultsTotal.get(totalPercentage).add(sb.toString());
                    resultsUnknown.get(unknownPercentage).add(sb.toString());
                }
            }
        }

        double maxTotalPercentage = Double.NEGATIVE_INFINITY;
        double maxUnknownPercentage = Double.NEGATIVE_INFINITY;
        for (double result : resultsTotal.keySet()) {
            if (result > maxTotalPercentage)
                maxTotalPercentage = result;
        }
        for (double result : resultsUnknown.keySet()) {
            if (result > maxUnknownPercentage)
                maxUnknownPercentage = result;
        }
        System.out.println();
        System.out.println("The highest total percentage is: " + maxTotalPercentage);
        for (String arguments : resultsTotal.get(maxTotalPercentage)) {
            System.out.println(arguments);
        }
        System.out.println();
        System.out.println("The highest unknown percentage is: " + maxUnknownPercentage);
        for (String arguments : resultsUnknown.get(maxUnknownPercentage)) {
            System.out.println(arguments);
        }
    }
}
