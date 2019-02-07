﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Internal;
using Xunit;

namespace NBB.EventStore.Tests
{
    public class SnapshotStoreTests
    {
        [Fact]
        public async Task Should_save_snapshot_in_snapshot_repository()
        {
            //Arrange
            var snapshotRepository = new Mock<ISnapshotRepository>();
            var sut = new SnapshotStore(snapshotRepository.Object, Mock.Of<IEventStoreSerDes>(), Mock.Of<ILogger<SnapshotStore>>());
            var snapshot = "snapshot";
            var stream = "stream";

            //Act
            await sut.StoreSnapshotAsync(new SnapshotEnvelope(snapshot, 5, stream));

            //Assert
            snapshotRepository.Verify(er => er.StoreSnapshotAsync(stream, It.IsAny<SnapshotDescriptor>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task Should_load_snapshot_in_snapshot_repository()
        {
            //Arrange
            var stream = "stream";
            var descriptor = new SnapshotDescriptor("aaa", "", stream, 5);
            var snapshotRepository = new Mock<ISnapshotRepository>();

            snapshotRepository.Setup(x => x.LoadSnapshotAsync(It.IsAny<string>(), CancellationToken.None))
                .Returns(Task.FromResult(descriptor));

            var eventSerDes = new Mock<IEventStoreSerDes>();
            var sut = new SnapshotStore(snapshotRepository.Object, eventSerDes.Object, Mock.Of<ILogger<SnapshotStore>>());

            //Act
            var snapshot = await sut.LoadSnapshotAsync(stream);

            //Assert
            snapshotRepository.Verify(er => er.LoadSnapshotAsync(stream, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
