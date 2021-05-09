﻿using AwesomeGithubStats.Core.Interfaces;
using AwesomeGithubStats.Core.Models;
using AwesomeGithubStats.Core.Models.Svgs;
using AwesomeGithubStats.Core.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TimeSpan = System.TimeSpan;

namespace AwesomeGithubStats.Core.Services
{
    class SvgService : ISvgService
    {
        private readonly string _contentRoot;
        private readonly ICacheService _cacheService;
        private readonly RankDegree _degree;
        private string SvgFolder { get; set; }
        private string TranslationFile { get; set; }

        public SvgService(
            IWebHostEnvironment environment,
            ICacheService cacheService,
            IOptions<RankDegree> rankDegree)
        {
            _degree = rankDegree.Value;
            _contentRoot = environment.ContentRootPath;
            _cacheService = cacheService;
            SvgFolder = Path.Combine(_contentRoot, @"\svgs");
            TranslationFile = Path.Combine(_contentRoot, @"\content", "translations.json");
        }



        public async Task<Stream> GetUserStatsImage(UserRank rank, UserStatsOptions options)
        {
            var file = await GetSvgFile("user-stats.svg");
            var svg = new UserStatsCard(file, _degree);

            var translations = options.HasTranslations() ? await GetTranslations(options.Locale) : await GetTranslations("en");

            return svg.Svg(rank, new CardStyles(), translations);
        }



        private async Task<CardStyles> GetStyle(string theme)
        {
            if (!File.Exists(TranslationFile))
                return new CardTranslations();

            var translations = _cacheService.Get<IEnumerable<CardTranslations>>(CacheKeys.TranslationKey);
            if (translations != null)
                return translations.Language(language);

            var jsonContent = await File.ReadAllTextAsync(TranslationFile);
            translations = JsonSerializer.Deserialize<IEnumerable<CardTranslations>>(jsonContent);

            _cacheService.Set(CacheKeys.TranslationKey, translations, TimeSpan.FromDays(30));

            return translations.Language(language);
        }
        private async Task<CardTranslations> GetTranslations(string language)
        {
            if (!File.Exists(TranslationFile))
                return new CardTranslations();

            var translations = _cacheService.Get<IEnumerable<CardTranslations>>(CacheKeys.TranslationKey);
            if (translations != null)
                return translations.Language(language);

            var jsonContent = await File.ReadAllTextAsync(TranslationFile);
            translations = JsonSerializer.Deserialize<IEnumerable<CardTranslations>>(jsonContent);

            _cacheService.Set(CacheKeys.TranslationKey, translations, TimeSpan.FromDays(30));

            return translations.Language(language);
        }
        private async Task<string> GetSvgFile(string file)
        {
            var svgContent = _cacheService.Get<string>(CacheKeys.SvgKey(file));
            if (!string.IsNullOrEmpty(svgContent))
                return svgContent;

            svgContent = await File.ReadAllTextAsync(Path.Combine(SvgFolder, "user-stats.svg"));

            _cacheService.Set(CacheKeys.SvgKey(file), svgContent, TimeSpan.FromDays(30));

            return svgContent;
        }
    }
}
