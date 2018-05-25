// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using DynamicData;

namespace Cinephile.Core.Model
{
    public interface IMovieService
    {
        IObservable<IChangeSet<Movie, int>> LoadUpcomingMovies(int index);
    }
}
