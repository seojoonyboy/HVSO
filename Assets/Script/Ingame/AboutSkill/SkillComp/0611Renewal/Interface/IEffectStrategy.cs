public interface IEffectStrategy {
    void SetTarget(object target);
    void SetArgs(object args);
    void Execute();
}
