using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PropertyFacadeExample.ViewModel
{
    public abstract class OneWayFacadeConverter<TSource, TFacade>
    {
        public abstract TFacade SourceToFacade(TSource sourceValue);
    }

    public abstract class TwoWayFacadeConverter<TSource, TFacade> : OneWayFacadeConverter<TSource, TFacade>
    {
        public abstract TSource FacadeToSource(TFacade facadeValue);
    }

    public class ObservableFacadeConverterAdapter<TSource, TFacade> : IObservable<TFacade>
    {
        private readonly IObservable<TSource> _observable;
        private readonly OneWayFacadeConverter<TSource, TFacade> _converter;

        public IDisposable Subscribe(IObserver<TFacade> observer) =>
            _observable
            .Select(sourceValue => _converter.SourceToFacade(sourceValue))
            .Subscribe(observer);

        public ObservableFacadeConverterAdapter(IObservable<TSource> observable, OneWayFacadeConverter<TSource, TFacade> converter)
        {
            _observable = observable ?? throw new ArgumentNullException(nameof(observable));
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }
    }

    public class SubjectFacadeConverterAdapter<TSource, TFacade> : ObservableFacadeConverterAdapter<TSource, TFacade>, ISubject<TFacade>
    {
        private readonly ISubject<TSource> _subject;
        private readonly TwoWayFacadeConverter<TSource, TFacade> _converter;

        public void OnNext(TFacade value) => _subject.OnNext(_converter.FacadeToSource(value));

        public void OnError(Exception error) => _subject.OnError(error);

        public void OnCompleted() => _subject.OnCompleted();

        public SubjectFacadeConverterAdapter(ISubject<TSource> subject, TwoWayFacadeConverter<TSource, TFacade> converter)
            : base(subject, converter)
        {
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }
    }

    public static class FacadeConverterExtensions
    {
        private sealed class OneWayFuncConverter<TSource, TFacade> : OneWayFacadeConverter<TSource, TFacade>
        {
            private readonly Func<TSource, TFacade> _sourceToFacade;

            public override TFacade SourceToFacade(TSource sourceValue) => _sourceToFacade.Invoke(sourceValue);

            public OneWayFuncConverter(Func<TSource, TFacade> sourceToFacade)
            {
                _sourceToFacade = sourceToFacade ?? throw new ArgumentNullException(nameof(sourceToFacade));
            }
        }

        private sealed class TwoWayFuncConverter<TSource, TFacade> : TwoWayFacadeConverter<TSource, TFacade>
        {
            private readonly Func<TSource, TFacade> _sourceToFacade;
            private readonly Func<TFacade, TSource> _facadeToSource;

            public override TFacade SourceToFacade(TSource sourceValue) => _sourceToFacade.Invoke(sourceValue);

            public override TSource FacadeToSource(TFacade facadeValue) => _facadeToSource.Invoke(facadeValue);

            public TwoWayFuncConverter(Func<TSource, TFacade> sourceToFacade, Func<TFacade, TSource> facadeToSource)
            {
                _sourceToFacade = sourceToFacade ?? throw new ArgumentNullException(nameof(sourceToFacade));
                _facadeToSource = facadeToSource ?? throw new ArgumentNullException(nameof(facadeToSource));
            }
        }

        public static SubjectFacadeConverterAdapter<TSource, TFacade> ConvertTwoWay<TSource, TFacade>(
            this ISubject<TSource> source,
            Func<TSource, TFacade> sourceToFacade,
            Func<TFacade, TSource> facadeToSource) =>
                new SubjectFacadeConverterAdapter<TSource, TFacade>(
                    subject: source,
                    converter: new TwoWayFuncConverter<TSource, TFacade>(
                        sourceToFacade: sourceToFacade,
                        facadeToSource: facadeToSource));

        public static ObservableFacadeConverterAdapter<TSource, TFacade> ConvertOneWay<TSource, TFacade>(
            this IObservable<TSource> source,
            Func<TSource, TFacade> sourceToFacade) =>
                new ObservableFacadeConverterAdapter<TSource, TFacade>(
                    observable: source,
                    converter: new OneWayFuncConverter<TSource, TFacade>(
                        sourceToFacade: sourceToFacade));

        public static SubjectFacadeConverterAdapter<TSource, TFacade> ConvertTwoWay<TSource, TFacade>(
            this ISubject<TSource> source,
            TwoWayFacadeConverter<TSource, TFacade> converter) =>
                new SubjectFacadeConverterAdapter<TSource, TFacade>(source, converter);

        public static ObservableFacadeConverterAdapter<TSource, TFacade> ConvertOneWay<TSource, TFacade>(
            this IObservable<TSource> source,
            OneWayFacadeConverter<TSource, TFacade> converter) =>
                new ObservableFacadeConverterAdapter<TSource, TFacade>(source, converter);
    }
}
