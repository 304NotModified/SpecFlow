﻿using System;
using System.Globalization;
using Gherkin.Ast;
using Moq;
using SpecFlow.ExternalData.SpecFlowPlugin.DataSource;
using SpecFlow.ExternalData.SpecFlowPlugin.Loaders;
using Xunit;

namespace SpecFlow.ExternalData.SpecFlowPlugin.UnitTests
{
    public class SpecificationProviderTests
    {
        private const string SOURCE_FILE_PATH = @"C:\Temp\Sample.feature";
        private readonly Mock<IDataSourceLoaderFactory> _dataSourceLoaderFactoryMock = new();
        private readonly Mock<IDataSourceLoader> _dataSourceLoaderMock = new();

        public SpecificationProviderTests()
        {
            _dataSourceLoaderFactoryMock.Setup(f => f.CreateLoader())
                                        .Returns(_dataSourceLoaderMock.Object);
        }
        
        private SpecificationProvider CreateSut()
        {
            return new(_dataSourceLoaderFactoryMock.Object);
        }

        [Fact]
        public void Should_return_null_if_no_data_source_tag()
        {
            var sut = CreateSut();

            var result = sut.GetSpecification(new[] { new Tag(null, @"@other-tag") }, SOURCE_FILE_PATH);
            
            Assert.Null(result);
        }

        [Theory]
        [InlineData("@DataSource:")]
        [InlineData("@DataSource")]
        public void Should_handle_invalid_data_source_tags(string tag)
        {
            var sut = CreateSut();

            Assert.Throws<ExternalDataPluginException>(() => sut.GetSpecification(new[]
            {
                new Tag(null, tag)
            }, SOURCE_FILE_PATH));
        }

        [Theory]
        [InlineData("@DataField")]
        [InlineData("@DataField:")]
        public void Should_handle_invalid_tags(string tag)
        {
            var sut = CreateSut();

            Assert.Throws<ExternalDataPluginException>(() => sut.GetSpecification(new[]
            {
                new Tag(null, "@DataSource:foo"),
                new Tag(null, tag)
            }, SOURCE_FILE_PATH));
        }

        [Fact]
        public void Should_pass_on_source_file_path()
        {
            var sut = CreateSut();

            var result = sut.GetSpecification(new[] { new Tag(null, @"@DataSource:path\to\file.csv") }, SOURCE_FILE_PATH);
            
            Assert.NotNull(result);
            _dataSourceLoaderMock.Verify(l => l.LoadDataSource(It.IsAny<string>(), SOURCE_FILE_PATH, It.IsAny<CultureInfo>()));
        }

        [Fact]
        public void Should_get_data_source_path_from_tags()
        {
            var sut = CreateSut();

            var result = sut.GetSpecification(new[] { new Tag(null, @"@DataSource:path\to\file.csv") }, SOURCE_FILE_PATH);
            
            Assert.NotNull(result);
            _dataSourceLoaderMock.Verify(l => l.LoadDataSource(@"path\to\file.csv", It.IsAny<string>(), It.IsAny<CultureInfo>()));
        }

        [Fact]
        public void Should_collect_field_mappings_from_tags()
        {
            var sut = CreateSut();

            var result = sut.GetSpecification(new[]
            {
                new Tag(null, @"@DataSource:path\to\file.csv"),
                new Tag(null, @"@DataField:target_field=source_field"),
            }, SOURCE_FILE_PATH);

            Assert.NotNull(result);
            Assert.NotNull(result.Fields);
            Assert.Contains("target_field", result.Fields.Keys);
            Assert.Equal("source_field", result.Fields["target_field"]);
        }

        [Fact]
        public void Should_use_last_DataField_setting_for_duplicated_target_fields()
        {
            var sut = CreateSut();

            var result = sut.GetSpecification(new[]
            {
                new Tag(null, @"@DataSource:path\to\file.csv"),
                new Tag(null, @"@DataField:target_field=source_field1"),
                new Tag(null, @"@DataField:target_field=source_field2"),
            }, SOURCE_FILE_PATH);

            Assert.NotNull(result);
            Assert.NotNull(result.Fields);
            Assert.Contains("target_field", result.Fields.Keys);
            Assert.Equal("source_field2", result.Fields["target_field"]);
        }

        [Theory]
        [InlineData("@DataField:target_field")]
        [InlineData("@DataField:target_field=")]
        public void Should_use_target_field_as_source_when_DataField_setting_does_not_contain_value(string dataFieldTag)
        {
            var sut = CreateSut();

            var result = sut.GetSpecification(new[]
            {
                new Tag(null, @"@DataSource:path\to\file.csv"),
                new Tag(null, dataFieldTag)
            }, SOURCE_FILE_PATH);

            Assert.NotNull(result);
            Assert.NotNull(result.Fields);
            Assert.Contains("target_field", result.Fields.Keys);
            Assert.Equal("target_field", result.Fields["target_field"]);
        }
    }
}
