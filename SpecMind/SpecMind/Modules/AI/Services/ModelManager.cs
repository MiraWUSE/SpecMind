using SpecMind.Modules.AI.Models;

namespace SpecMind.Modules.AI.Services;

public class ModelManager
{
    private readonly List<AIModel> _models = new();

    public IReadOnlyList<AIModel> Models => _models;

    public void Register(AIModel model)
    {
        if (_models.Any(x => x.Path == model.Path))
            return;

        _models.Add(model);
    }

    public AIModel? GetActiveModel()
    {
        return _models.FirstOrDefault(x => x.IsActive);
    }

    public void SetActive(AIModel model)
    {
        foreach (var item in _models)
            item.IsActive = false;

        model.IsActive = true;
    }

    public bool HasModels()
    {
        return _models.Count > 0;
    }
}