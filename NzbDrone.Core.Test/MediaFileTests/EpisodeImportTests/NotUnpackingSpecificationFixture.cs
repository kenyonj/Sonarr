﻿using System;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFileTests.EpisodeImportTests
{
    [TestFixture]
    public class NotUnpackingSpecificationFixture : CoreTest<NotUnpackingSpecification>
    {
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.DownloadClientWorkingFolders)
                .Returns("_UNPACK_|_FAILED_");

            _localEpisode = new LocalEpisode
            {
                Path = @"C:\Test\Unsorted TV\30.rock\30.rock.s01e01.avi".AsOsAgnostic(),
                Size = 100,
                Series = Builder<Series>.CreateNew().Build()
            };
        }

        private void GivenInWorkingFolder()
        {
            _localEpisode.Path = @"C:\Test\Unsorted TV\_UNPACK_30.rock\30.rock.s01e01.avi".AsOsAgnostic();
        }

        private void GivenLastWriteTimeUtc(DateTime time)
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetLastFileWrite(It.IsAny<string>()))
                .Returns(time);
        }

        [Test]
        public void should_return_true_if_not_in_working_folder()
        {
            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_in_old_working_folder()
        {
            GivenInWorkingFolder();
            GivenLastWriteTimeUtc(DateTime.UtcNow.AddHours(-1));

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_in_working_folder_and_last_write_time_was_recent()
        {
            GivenInWorkingFolder();
            GivenLastWriteTimeUtc(DateTime.UtcNow);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
        }
    }
}