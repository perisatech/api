using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using inventory.Controllers;
using inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace inventory.Tests
{
    public class WeatherForecastTests
    {
        private readonly WeatherForecastController _controller;
        private readonly Mock<ILogger<WeatherForecastController>> _loggerMock;

        public WeatherForecastTests()
        {
            _loggerMock = new Mock<ILogger<WeatherForecastController>>();
            _controller = new WeatherForecastController(_loggerMock.Object);
        }

        [Fact]
        public void Get_ReturnsWeatherForecasts()
        {
            // Act
            var result = _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var forecasts = Assert.IsAssignableFrom<IEnumerable<WeatherForecast>>(okResult.Value);
            Assert.Equal(5, forecasts.Count());
        }

        [Fact]
        public void Get_ReturnsNotFound_WhenNoForecasts()
        {
            // Arrange
            // You may need to adjust the controller to handle this case

            // Act
            var result = _controller.Get();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}