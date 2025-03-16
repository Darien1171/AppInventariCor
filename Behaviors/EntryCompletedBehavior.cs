using System.Windows.Input;
using Microsoft.Maui.Controls;
using AppInventariCor.Models;

namespace AppInventariCor.Behaviors
{
    public class EntryCompletedBehavior : Behavior<Entry>
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EntryCompletedBehavior));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(EntryCompletedBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            entry.Completed += OnEntryCompleted;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            base.OnDetachingFrom(entry);
            entry.Completed -= OnEntryCompleted;
        }

        private void OnEntryCompleted(object sender, EventArgs args)
        {
            if (Command != null && sender is Entry entry && CommandParameter is Repuesto repuesto)
            {
                var param = new Tuple<Repuesto, string>(repuesto, entry.Text);
                if (Command.CanExecute(param))
                {
                    Command.Execute(param);
                }
            }
        }
    }
}