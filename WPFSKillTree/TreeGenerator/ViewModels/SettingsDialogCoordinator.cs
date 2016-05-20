using System.Collections.Generic;
using System.Threading.Tasks;
using POESKillTree.Controls.Dialogs;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Solver;
using POESKillTree.TreeGenerator.Views;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public interface ISettingsDialogCoordinator : IDialogCoordinator
    {
        Task<IEnumerable<ushort>> ShowControllerDialogAsync(object context, ISolver solver, string generatorName, SkillTree tree);
    }

    public class SettingsDialogCoordinator : DialogCoordinator, ISettingsDialogCoordinator
    {
        public new static readonly SettingsDialogCoordinator Instance = new SettingsDialogCoordinator();

        public async Task<IEnumerable<ushort>> ShowControllerDialogAsync(object context, ISolver solver, string generatorName, SkillTree tree)
        {
            var vm = new ControllerViewModel(solver, generatorName, tree, this);
            var view = new ControllerWindow();
            var task = vm.RunSolverAsync();
            await ShowDialogAsync(context, vm, view, hideOnRequestsClose: false);
            var result = await task;
            await HideDialogAsync(context, view);
            return result;
        }
    }
}