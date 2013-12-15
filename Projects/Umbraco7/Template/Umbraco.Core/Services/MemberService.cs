﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using System.Linq;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the MemberService.
    /// </summary>
    internal class MemberService : IMemberService
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        public MemberService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        public MemberService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        {
        }

        public MemberService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _uowProvider = provider;
        }

        #region IMemberService Implementation

        /// <summary>
        /// Checks if a member with the username exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool Exists(string username)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Exists(username);
            }
        }

        /// <summary>
        /// Checks if a member with the id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exists(int id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Exists(id);
            }
        }

        /// <summary>
        /// Gets a Member by its integer Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMember GetById(int id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {                
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a Member by its Guid key
        /// </summary>
        /// <remarks>
        /// The guid key corresponds to the unique id in the database
        /// and the user id in the membership provider.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMember GetByKey(Guid id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.Key == id);
                var member = repository.GetByQuery(query).FirstOrDefault();
                return member;
            }
        }

        /// <summary>
        /// Gets a list of Members by their MemberType
        /// </summary>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByMemberType(string memberTypeAlias)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.ContentTypeAlias == memberTypeAlias);
                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members by the MemberGroup they are part of
        /// </summary>
        /// <param name="memberGroupName"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByGroup(string memberGroupName)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetByMemberGroup(memberGroupName);
            }
        }

        /// <summary>
        /// Gets a list of all Members
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetAllMembers(params int[] ids)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Does a search for members that contain the specified string in their email address
        /// </summary>
        /// <param name="emailStringToMatch"></param>
        /// <returns></returns>
        public IEnumerable<IMember> FindMembersByEmail(string emailStringToMatch)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                var query = new Query<IMember>();


                query.Where(member => member.Email.Contains(emailStringToMatch));

                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain string property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, string value)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query =
                    Query<IMember>.Builder.Where(
                        x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            (((Member)x).LongStringPropertyValue.Contains(value) ||
                             ((Member)x).ShortStringPropertyValue.Contains(value)));

                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain integer property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, int value)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query =
                    Query<IMember>.Builder.Where(
                        x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias && 
                            ((Member)x).IntegerropertyValue == value);

                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain boolean property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, bool value)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query =
                    Query<IMember>.Builder.Where(
                        x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).BoolPropertyValue == value);

                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain date time property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, DateTime value)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query =
                    Query<IMember>.Builder.Where(
                        x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).DateTimePropertyValue == value);

                var members = repository.GetByQuery(query);
                return members;
            }
        }
        
        #endregion

        #region IMembershipMemberService Implementation

        /// <summary>
        /// Creates a new Member
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="memberTypeAlias"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IMember CreateMember(string email, string username, string password, string memberTypeAlias, int userId = 0)
        {
            var uow = _uowProvider.GetUnitOfWork();
            IMemberType memberType;

            using (var repository = _repositoryFactory.CreateMemberTypeRepository(uow))
            {
                var query = Query<IMemberType>.Builder.Where(x => x.Alias == memberTypeAlias);
                memberType = repository.GetByQuery(query).FirstOrDefault();
            }

            if (memberType == null)
                throw new Exception(string.Format("No MemberType matching the passed in Alias: '{0}' was found", memberTypeAlias));

            var member = new Member(email, email, username, password, -1, memberType);

            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.AddOrUpdate(member);
                uow.Commit();

                //insert the xml
                var xml = member.ToXml();
                CreateAndSaveContentXml(xml, member.Id, uow.Database);
            }

            return member;
        }

        /// <summary>
        /// Gets a Member by its Id
        /// </summary>
        /// <remarks>
        /// The Id should be an integer or Guid.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMember GetById(object id)
        {
            if (id is int)
            {
                return GetById((int)id);
            }

            if (id is Guid)
            {
                return GetByKey((Guid)id);
            }

            return null;
        }

        /// <summary>
        /// Gets a Member by its Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public IMember GetByEmail(string email)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                var escapedEmail = uow.Database.EscapeAtSymbols(email);
                var query = Query<IMember>.Builder.Where(x => x.Email == escapedEmail);
                var member = repository.GetByQuery(query).FirstOrDefault();

                return member;
            }
        }

        /// <summary>
        /// Gets a Member by its Username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IMember GetByUsername(string userName)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                var escapedUser = uow.Database.EscapeAtSymbols(userName);
                var query = Query<IMember>.Builder.Where(x => x.Username == escapedUser);
                var member = repository.GetByQuery(query).FirstOrDefault();

                return member;
            }
        }

        /// <summary>
        /// Deletes a Member
        /// </summary>
        /// <param name="member"></param>
        public void Delete(IMember member)
        {
            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMember>(member), this))
                return;

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.Delete(member);
                uow.Commit();
            }

            Deleted.RaiseEvent(new DeleteEventArgs<IMember>(member, false), this);
        }

        /// <summary>
        /// Saves an updated Member
        /// </summary>
        /// <param name="member"></param>
        /// <param name="raiseEvents"></param>
        public void Save(IMember member, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(member), this))
                    return;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.AddOrUpdate(member);
                uow.Commit();

                //insert the xml
                var xml = member.ToXml();
                CreateAndSaveContentXml(xml, member.Id, uow.Database);
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMember>(member, false), this);
        }

        #endregion

        private void CreateAndSaveContentXml(XElement xml, int id, UmbracoDatabase db)
        {
            var contentPoco = new ContentXmlDto { NodeId = id, Xml = xml.ToString(SaveOptions.None) };
            var contentExists = db.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId = @Id", new { Id = id }) != 0;
            int contentResult = contentExists ? db.Update(contentPoco) : Convert.ToInt32(db.Insert(contentPoco));
        }

        #region Event Handlers


        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IMemberService, DeleteEventArgs<IMember>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IMemberService, DeleteEventArgs<IMember>> Deleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IMemberService, SaveEventArgs<IMember>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IMemberService, SaveEventArgs<IMember>> Saved;
        
        #endregion

        /// <summary>
        /// A helper method that will create a basic/generic member for use with a generic membership provider
        /// </summary>
        /// <returns></returns>
        internal static IMember CreateGenericMembershipProviderMember(string name, string email, string username, string password)
        {
            var identity = int.MaxValue;

            var memType = new MemberType(-1);
            var propGroup = new PropertyGroup
                {
                    Name = "Membership",
                    Id = --identity
                };
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext)
                {
                    Alias = Constants.Conventions.Member.Comments,
                    Name = Constants.Conventions.Member.CommentsLabel,
                    SortOrder = 0,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer)
                {
                    Alias = Constants.Conventions.Member.IsApproved,
                    Name = Constants.Conventions.Member.IsApprovedLabel,
                    SortOrder = 3,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer)
                {
                    Alias = Constants.Conventions.Member.IsLockedOut,
                    Name = Constants.Conventions.Member.IsLockedOutLabel,
                    SortOrder = 4,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastLockoutDate,
                    Name = Constants.Conventions.Member.LastLockoutDateLabel,
                    SortOrder = 5,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastLoginDate,
                    Name = Constants.Conventions.Member.LastLoginDateLabel,
                    SortOrder = 6,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastPasswordChangeDate,
                    Name = Constants.Conventions.Member.LastPasswordChangeDateLabel,
                    SortOrder = 7,
                    Id = --identity
                });

            memType.PropertyGroups.Add(propGroup);

            return new Member(name, email, username, password, -1, memType);
        }
    }
}