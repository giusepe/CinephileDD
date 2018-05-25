using System;
using System.Globalization;
using System.Linq;
using CinephileDD.Core.Model;
using CinephileDD.Core.Rest.Dtos.Movies;

namespace CinephileDD.Core.Models
{   
    public static class MovieMapper
    {
        const string BaseUrl = "http://image.tmdb.org/t/p/";
        const string SmallPosterSize = "w185";
        const string BigPosterSize = "w500";

		public static Movie ToModel(GenresDto genres, MovieResult movieDto, string language)
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
				ReleaseDate = DateTime.Parse(movieDto.ReleaseDate, new CultureInfo(language)),
                Overview = movieDto.Overview
            };
        }
    }
}
