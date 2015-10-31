package edu.berkeley.nlp.math;

/**
 * @author Dan Klein
 */
public interface GradientLineSearcher {
  double[] minimize(DifferentiableFunction function, double[] initial, double[] direction);
}
