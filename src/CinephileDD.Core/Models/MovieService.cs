using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using CinephileDD.Core.Rest;
using CinephileDD.Core.Rest.Dtos.Movies;
using DynamicData;
using Splat;

namespace CinephileDD.Core.Model
{
    public class MovieService : IMovieService
    {
        public const int PageSize = 20;
        const string Language = "en-US";

        const string BaseUrl = "http://image.tmdb.org/t/p/";
        const string SmallPosterSize = "w185";
        const string BigPosterSize = "w500";

        public IObservableCache<Movie, int> UpcomingMovies { get; private set; }

        private const string apiKey = "1f54bd990f1cdfb230adb312546d765d";
        private IApiService movieApiService;
        private ICache movieCache;

        public MovieService(IApiService apiService = null, ICache cache = null)
        {
            movieApiService = apiService ?? Locator.Current.GetService<IApiService>();
            movieCache = cache ?? Locator.Current.GetService<ICache>();

            UpcomingMovies = LoadUpcomingMovies(-1)
                .AsObservableCache();
        }

        public IObservable<IChangeSet<Movie, int>> LoadUpcomingMovies(int index)
        {
            return ObservableChangeSet.Create<Movie, int>(cache =>
            {
                var disposable = movieCache
                    .GetAndFetchLatest($"upcoming_movies_{index}", () => FetchUpcomingMovies(index))
                    .SelectMany(x => x)
                    .Do(x => System.Diagnostics.Debug.WriteLine($"========> Movie {x.Id} - {x.Title}"))
                    .Subscribe(x => cache.AddOrUpdate(x));

                return disposable;

            }, movie => movie.Id)
            .DeferUntilLoaded();
        }

        IObservable<IEnumerable<Movie>> FetchUpcomingMovies(int index)
        {
            int page = (int)Math.Ceiling(index / (double)PageSize) + 1;

            return Observable
                .CombineLatest(
                    movieApiService
                        .UserInitiated
                        .FetchUpcomingMovies(apiKey, page, Language),
                    movieApiService
                        .UserInitiated
                        .FetchGenres(apiKey, Language),
                    (movies, genres) =>
                    {
                        return movies
                                .Results
                                .Select(movieDto => MapDtoToModel(genres, movieDto));
                    });
        }

        Movie MapDtoToModel(GenresDto genres, MovieResult movieDto)
        {
            return new Movie()
            {
                Id = movieDto.Id,
                Title = movieDto.Title,
                PosterSmall = string
                                .Concat(BaseUrl,
                                       SmallPosterSize,
                                       movieDto.PosterPath),
                PosterBig = string
                                .Concat(BaseUrl,
									   BigPosterSize,
		                               movieDto.PosterPath),
                Genres = genres.Genres.Where(g => movieDto.GenreIds.Contains(g.Id)).Select(j => j.Name).ToList(),
                ReleaseDate = DateTime.Parse(movieDto.ReleaseDate, new CultureInfo(Language)),
                Overview = movieDto.Overview
            };
        }
    }
}