﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;

namespace Tests.Users
{
    [TestClass]
    public class UserTests : TestsBase
    {
        private readonly UserStepDefinitions _u;

        public UserTests()
        {
            _u = new UserStepDefinitions();
        }

        [TestMethod]
        public async Task GetAll_ReturnsEmptyList_WhenThereAreNoUsers()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _u.WHEN_AllUsersAreRequested();

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _u.THEN_ResponseContainsNoUsers(response);
        }

        [TestMethod]
        public async Task GetAll_ReturnsAllUsers_WhenThereIsOneUser()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk");

            // Act
            var response = await _u.WHEN_AllUsersAreRequested();

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _u.THEN_ResponseContainsTheFollowingUsers(response, "henk");
        }

        [TestMethod]
        public async Task GetAll_ReturnsAllUsers_WhenThereAreMultipleUsers()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk", "miep");

            // Act
            var response = await _u.WHEN_AllUsersAreRequested();

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _u.THEN_ResponseContainsTheFollowingUsers(response, "henk", "miep");
        }

        [TestMethod]
        public async Task GetOne_ReturnsAUser_WhenTheUserExists()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk");

            // Act
            var response = await _u.WHEN_OneUserIsRequested("henk");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _u.THEN_ResponseContainsTheFollowingUsers(response, "henk");
        }

        [TestMethod]
        public async Task GetOne_ReturnsAUser_WhenTheUserExists_DifferentCasing()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk");

            // Act
            var response = await _u.WHEN_OneUserIsRequested("hEnK");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.OK);
            await _u.THEN_ResponseContainsTheFollowingUsers(response, "henk");
        }

        [TestMethod]
        public async Task GetOne_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk");

            // Act
            var response = await _u.WHEN_OneUserIsRequested("miep");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetOne_ReturnsBadRequest_WhenUsernameTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var veryLongUsername = new string('A', 101);
            var response = await _u.WHEN_OneUserIsRequested(veryLongUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddUser_ReturnsBadRequest_WhenUsernameNull()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _u.WHEN_UserIsCreated(null);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddUser_ReturnsBadRequest_WhenUsernameEmpty()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _u.WHEN_UserIsCreated(string.Empty);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddUser_ReturnsBadRequest_WhenUsernameTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongUsername = new string('A', 101);

            // Act
            var response = await _u.WHEN_UserIsCreated(veryLongUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddUser_ReturnsBadRequest_WhenRequestIsNotAUser()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var json = "{ \"this is\": \"no user\" }";

            // Act
            var response = await _u.WHEN_UserIsCreatedWithJsonString(json);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AddUser_ReturnsConflict_WhenUserAlreadyExists()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk");

            // Act
            var response = await _u.WHEN_UserIsCreated("henk");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task AddUser_ReturnsConflict_WhenUserAlreadyExists_DifferentCasing()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk");

            // Act
            var response = await _u.WHEN_UserIsCreated("hEnK");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task AddUser_CreatesUser()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _u.WHEN_UserIsCreated("henk123");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Created);
            THEN_ResponseHasLocationHeader(response, "/users/henk123");
            await _u.THEN_TheFollowingUsersExist("henk123");
        }

        [TestMethod]
        public async Task AddUser_CreatesUser_UsernameHasMaxLength()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongUsername = new string('A', 100);

            // Act
            var response = await _u.WHEN_UserIsCreated(veryLongUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.Created);
            THEN_ResponseHasLocationHeader(response, $"/users/{veryLongUsername}");
            await _u.THEN_TheFollowingUsersExist(veryLongUsername);
        }

        [TestMethod]
        public async Task DeleteUser_ReturnsBadRequest_WhenUsernameTooLong()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();
            var veryLongUsername = new string('A', 101);

            // Act
            var response = await _u.WHEN_UserIsDeleted(veryLongUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task DeleteUser_ReturnsMethodNotAllowed_WhenUsernameContainsInvalidCharacters()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _u.WHEN_UserIsDeleted("1@6*");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task DeleteUser_ReturnsNoContent_WhenUserDoesNotExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _u.WHEN_UserIsDeleted("henk");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
        }

        [TestMethod]
        public async Task DeleteUser_DeletesUser()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk");

            // Act
            var response = await _u.WHEN_UserIsDeleted("henk");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _u.THEN_NoUsersExist();
        }

        [TestMethod]
        public async Task DeleteUser_DeletesUser_DifferentCase()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk");

            // Act
            var response = await _u.WHEN_UserIsDeleted("hEnK");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _u.THEN_NoUsersExist();
        }

        [TestMethod]
        public async Task DeleteUser_DeletesUser_UsernameHasMaxLength()
        {
            // Arrange
            var veryLongUsername = new string('A', 100);
            await _u.GIVEN_ThereAreTheFollowingUsers(veryLongUsername);

            // Act
            var response = await _u.WHEN_UserIsDeleted(veryLongUsername);

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _u.THEN_NoUsersExist();
        }

        [TestMethod]
        public async Task DeleteUser_DeletesUser_OtherUsersStillExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk", "miepje");

            // Act
            var response = await _u.WHEN_UserIsDeleted("henk");

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _u.THEN_TheFollowingUsersExist("miepje");
        }

        [TestMethod]
        public async Task DeleteAllUsers_ReturnsNoContent_WhenNoUsersExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreNoUsers();

            // Act
            var response = await _u.WHEN_AllUsersAreDeleted();

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _u.THEN_NoUsersExist();
        }

        [TestMethod]
        public async Task DeleteAllUsers_DeletesAllUsers_WhenUsersExist()
        {
            // Arrange
            await _u.GIVEN_ThereAreTheFollowingUsers("henk", "miep");

            // Act
            var response = await _u.WHEN_AllUsersAreDeleted();

            // Assert
            THEN_ResponseHasStatusCode(response, HttpStatusCode.NoContent);
            await _u.THEN_NoUsersExist();
        }
    }
}