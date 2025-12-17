using System;
using System.Diagnostics.CodeAnalysis;

using ReactiveUI;

namespace ProcessManagerViewer;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IViewLocator {
    /// </inheritdoc>
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null) {
        if (viewModel is null) {
            return null;
        }

        var name = viewModel.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type is null) {
            return new Views.ViewNotFoundView {
                DataContext = new ViewModels.ViewNotFoundViewModel($"View not found: {name}")
            };
        }

        var control = (IViewFor)Activator.CreateInstance(type)!;
        control.ViewModel = viewModel;
        return control;
    }
}
