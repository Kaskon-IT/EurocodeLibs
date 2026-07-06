using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CommonLibrary.Models
{

    public abstract class BaseBerekening<TInput, TResult> : IBerekening<TInput, TResult>, INotifyPropertyChanged
        where TInput : INotifyPropertyChanged
    {
        public abstract string Naam { get; }

        private TInput _input;
        public TInput Input
        {
            get => _input;
            set
            {
                var oud = _input;
                if (SetProperty(ref _input, value))
                {
                    if (oud is not null)
                        oud.PropertyChanged -= OnInputChanged;
                    if (_input is not null)
                        _input.PropertyChanged += OnInputChanged;
                    MarkeerResultaatVerouderd();
                }
            }
        }

        private TResult? _result;
        public TResult? Result
        {
            get => _result;
            protected set => SetProperty(ref _result, value);
        }

        private bool _isResultStale;
        public bool IsResultStale
        {
            get => _isResultStale;
            protected set => SetProperty(ref _isResultStale, value);
        }

        public List<TexFormula> Formules { get; } = [];
        IReadOnlyList<TexFormula> IBerekening<TInput, TResult>.Formules => Formules;

        protected BaseBerekening(TInput input)
        {
            _input = input;
            if (_input is not null)
                _input.PropertyChanged += OnInputChanged;
        }

        public TResult Bereken()
        {
            Formules.Clear();
            Result = BerekenInternal();
            IsResultStale = false;
            OnUpdated();
            return Result;
        }

        protected abstract TResult BerekenInternal();

        private void OnInputChanged(object? sender, PropertyChangedEventArgs e)
            => MarkeerResultaatVerouderd();

        private void MarkeerResultaatVerouderd()
        {
            if (Result is not null)
                IsResultStale = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? Updated;

        protected void OnUpdated() => Updated?.Invoke();

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }


    }

}
