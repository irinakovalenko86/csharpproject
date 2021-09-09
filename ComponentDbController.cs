using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Swr.Configurator.Common.Data;
using Swr.Configurator.Common.Data.ObjectModel;
using SWR.ConfiguratorApi.Data.Db;
using SWR.ConfiguratorApi.Logic.Converters;
using Attribute = SWR.ConfiguratorApi.Data.Db.Attribute;

namespace SWR.ConfiguratorApi.Logic
{
    public static class ComponentDbController
    {
        private const string AddComponentQuery = @"INSERT INTO component (id, code, name, typeid, createdate, changedate, version, revision, stateid)
                                          VALUES (@Id, @Code, @Name, @TypeId, @CreateDate, @ChangeDate, @Version, @Revision, @StateId)";

        private const string AddComponentTypeQuery = @"INSERT INTO componenttype 
                                          VALUES (@Id, @Name, @NameTemplate, @Icon)";

        private const string AddComponentTypeAttributeQuery =
            @"INSERT INTO componenttypeattribute VALUES (@Id, @ComponentTypeId, @AttributeId, @Necessary, @Order, @Alias)";

        private const string GetComponentByIdQuery =
            @"SELECT *
            FROM (SELECT * FROM component WHERE component.id = @Id) AS comp 
            LEFT JOIN componentattribute AS compattr ON comp.id = compattr.componentid
	        LEFT JOIN componenttypeattribute AS comptypeattr ON compattr.componenttypeattributeid = comptypeattr.id
	        LEFT JOIN attribute AS attr ON comptypeattr.attributeid = attr.id";
         //   @"SELECT comp.code AS Code, comp.name AS Name, attr.name AS AttributeName, compattr.value AS AttributeValue
         //   FROM (SELECT * FROM component WHERE component.id = @Id) AS comp 
         //   LEFT JOIN componentattribute AS compattr ON comp.id = compattr.componentid
	        //LEFT JOIN componenttypeattribute AS comptypeattr ON compattr.componenttypeattributeid = comptypeattr.id
	        //LEFT JOIN attribute AS attr ON comptypeattr.attributeid = attr.id";

            private const string  AddComponentAttributesQuery =
        "INSERT INTO componentattribute(id, componentId, componenttypeattributeid, value) VALUES(@Id, @ComponentId, @ComponentTypeAttributeId, @Value)";


        private const string UpdateComponentQuery = @"UPDATE component SET code= @Code, name = @Name, typeid = @TypeId, version = @Version, revision = @Revision, stateid = @StateId, changedate = @ChangeDate WHERE id = @Id";

        private const string UpdateComponentAttributeQuery = @"UPDATE componentattribute SET value = @Value WHERE id = @Id";

        private const string UpdateComponentTypeQuery = @"UPDATE componenttype SET name = @Name, nametemplate=@NameTemplate, icon = @Icon WHERE id = @Id";

        private const string UpdateComponentTypeAttributeQuery =
            "UPDATE componenttypeattribute SET componenttypeid = @ComponentTypeId, attributeid = @AttributeId, necessary = @Necessary, \"order\" = @Order, alias = @Alias, isdeleted = @IsDeleted WHERE id=@Id";

        public static List<Component> GetComponents(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query = "select * from component";

            return connection.Query<Component>(query).ToList();
        }

        public static void ClearComponents(IDbConnection connection, IDbTransaction tr)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Query("delete from componentattribute", tr);
            connection.Query("delete from component", tr);
        }

        public static void DeleteComponent(IDbConnection connection, IDbTransaction tr, Guid id)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Query("delete from componentattribute where componentid = '" + id.ToString() + "'", id, tr);
            connection.Query("delete from component where id = '" + id.ToString()+ "'", id, tr);
        }

        public static void AddComponents(IDbConnection connection, IDbTransaction tr, IEnumerable<Component> components)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

			connection.Execute(AddComponentQuery, components, tr);
			var allComponentAttributes = components.SelectMany(t => t.ComponentAttributes).ToList();
			connection.Execute(AddComponentAttributesQuery, allComponentAttributes/*component.ComponentAttributes*/, tr);
		}

        public static void AddComponentTypes(IDbConnection connection, IDbTransaction tr, List<ComponentType> componentTypes)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Execute(AddComponentTypeQuery, componentTypes, tr);

        }

        public static void ClearComponentTypes(IDbConnection connection, IDbTransaction tr)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Query("delete from componenttype", tr);
        }

        public static Component GetComponentById(IDbConnection connection, IDbTransaction tr, Guid id)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var componentDictionary = new Dictionary<Guid, Component>();

            //var component =
                connection.Query<Component, ComponentAttribute, ComponentTypeAttribute, Attribute, Guid>(
                    GetComponentByIdQuery,
                    (component1, componentAttribute, componentTypeAttribute, attribute) =>
                    {
                        if (componentDictionary.ContainsKey(component1.Id))
                        {
                            if (attribute != null)
                            {
                                componentAttribute.Name = attribute.Name;
                                componentDictionary[component1.Id].ComponentAttributes.Add(componentAttribute);
                            }
                        }
                        else
                        {
                            if (attribute != null)
                            {
                                componentAttribute.Name = attribute.Name;
                                component1.ComponentAttributes.Add(componentAttribute);
                                
                            }
                            componentDictionary.Add(component1.Id, component1);
                        }
                        //componentAttribute.Name = attribute.Name;
                        //component1.ComponentAttributes.Add(componentAttribute);
                        return component1.Id;
                    }, new {Id = id}, tr, commandType: CommandType.Text, splitOn: "Id");//.First();

            var component = componentDictionary.Select(t => t.Value).First();
            return component;
            //connection.Execute(GetComponentByIdQuery, id, tr);
        }


        public static void SaveComponent(IDbConnection connection, IDbTransaction tr, IEnumerable<Component> components)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));


            connection.Execute(UpdateComponentQuery, components, tr);

            var newComponentAttributes =
                components.SelectMany(t => t.ComponentAttributes.Where(l => l.IsNew)).ToList();

            if (newComponentAttributes != null && newComponentAttributes.Count != 0)
            {
                connection.Execute(AddComponentAttributesQuery, newComponentAttributes, tr);
            }

            var allChangedComponentAttributes =
                components.SelectMany(t => t.ComponentAttributes.Where(k => !k.IsNew)).ToList();
            if (allChangedComponentAttributes != null && allChangedComponentAttributes.Count != 0) connection.Execute(UpdateComponentAttributeQuery, allChangedComponentAttributes, tr);

        }

        public static void AddComponentTypeAttributes(IDbConnection connection, IDbTransaction tr, List<ComponentTypeAttribute> componentTypeAttributes)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Execute(AddComponentTypeAttributeQuery, componentTypeAttributes, tr);
        }

        public static void ClearComponentTypeAttributes(IDbConnection connection, IDbTransaction tr)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Query("truncate table componenttypeattribute cascade", tr);
        }

        public static List<Component> SearchComponents(IDbConnection connection, IDbTransaction tr, SearchParams paramList, out int foundComponentCount)
        {
            var result = new List<Component>();
            foundComponentCount = 0;

            var count = paramList.Count == 0 ? 10 : paramList.Count; // возвращаемое количество
            var offset = paramList.PageNumber * count; // смещение

            var componentDictionary = new Dictionary<Guid, Component>();
            var searchComponentQuery = @"select *
            from (select * from component where id in (";//SELECT comp.id   FROM component AS comp";
         //   LEFT JOIN componentattribute AS compattr ON comp.id = compattr.componentid
	        //LEFT JOIN componenttypeattribute AS comptypeattr ON compattr.componenttypeattributeid = comptypeattr.id
	        //LEFT JOIN attribute AS attr ON comptypeattr.attributeid = attr.id";

            var innerQuery = @"SELECT comp.id   FROM component AS comp";
            int i = 0;
            foreach (var paramItem in paramList.SearchAttributes)
            {
                
                if (i == 0)
                {
                    innerQuery +=
                        @" JOIN componentattribute AS compattr ON comp.id = compattr.componentid  JOIN componenttypeattribute AS comptypeattr ON compattr.componenttypeattributeid = comptypeattr.id  JOIN attribute AS attr ON comptypeattr.attributeid = attr.id";
                }
                else
                {

                    innerQuery +=
                        @" JOIN componentattribute AS compattr" + i + " ON comp.id = compattr" + i + ".componentid  JOIN componenttypeattribute AS comptypeattr" + i + " ON compattr" + i + ".componenttypeattributeid = comptypeattr" + i + ".id  JOIN attribute AS attr" + i + " ON comptypeattr" + i + ".attributeid = attr" + i + ".id";
                }

                i++;
            }

            innerQuery += " where ";
            i = 0;
            foreach (var paramItem in paramList.SearchAttributes)
            {
                if (i == 0)
                {
                    innerQuery += @"attr.name = '" + paramItem.Name + "' and lower(compattr.value) like '%" + paramItem.Value.ToLower() + "%' ";
                }
                else
                {

                    innerQuery += " and ";
                    innerQuery += @"attr" + i + ".name = '" + paramItem.Name + "' and lower(compattr" + i +
                                            ".value) like '%" + paramItem.Value.ToLower() + "%'";
                }
                i++;

            }

            searchComponentQuery += innerQuery;
            searchComponentQuery += @") order by name limit " + count + " offset " + offset;
            searchComponentQuery += @") as comp
	JOIN componentattribute as compattr on comp.id = compattr.componentid
	JOIN componenttypeattribute as comptypeattr on compattr.componenttypeattributeid = comptypeattr.id
	JOIN attribute as attr on comptypeattr.attributeid = attr.id";

            //var queryResult = connection.Query(searchComponentQuery, transaction: tr, commandType: CommandType.Text);
            var guids = connection.Query<Component, ComponentAttribute, ComponentTypeAttribute, Attribute, Guid>(searchComponentQuery,
                (component, componentAttribute, componentTypeAttribute, attribute) =>
                {
                    if (componentDictionary.ContainsKey(component.Id))
                    {
                        if (attribute != null) componentAttribute.Name = attribute.Name;
                        componentDictionary[component.Id].ComponentAttributes.Add(componentAttribute);
                    }
                    else
                    {
                        if (attribute != null) componentAttribute.Name = attribute.Name;
                        component.ComponentAttributes.Add(componentAttribute);
                        componentDictionary.Add(component.Id, component);
                    }
                    //componentAttribute.Name = attribute.Name;
                    //component1.ComponentAttributes.Add(componentAttribute);
                    return component.Id;
                }, transaction: tr,  commandType: CommandType.Text, splitOn: "id");

            if (paramList.PageNumber == 0)
            {
                foundComponentCount = connection.Query<Guid>(innerQuery, transaction: tr, commandType: CommandType.Text)
                    .ToList().Count;
            }

            result = componentDictionary.Select(t => t.Value).ToList();
            //result = result.OrderBy(t => t.Name).ToList();
            return result;
        }



        public static void UpdateComponentTypes(IDbConnection connection, IDbTransaction tr, List<ComponentType> componentTypes)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Execute(UpdateComponentTypeQuery, componentTypes, tr);
        }

        public static void UpdateComponentTypeAttributes(IDbConnection connection, IDbTransaction tr,
            List<ComponentTypeAttribute> componentTypeAttributes)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Execute(UpdateComponentTypeAttributeQuery, componentTypeAttributes, tr);
        }

        public static IEnumerable<ComponentType> GetComponentTypes(IDbConnection connection, IDbTransaction tr)
        {
            var result = new List<ComponentType>();

            var componentTypeDictionary = new Dictionary<Guid, ComponentType>();

            var query =
                @"select comptype.*, comptypeattr.*, attr.*, typerel.* from componenttype as comptype left join componenttypeattribute as comptypeattr on comptype.id = comptypeattr.componenttypeid
				left join attribute as attr on comptypeattr.attributeid = attr.id
            left join (select typerelation.*, specificationtype.* from typerelation 
            join specificationtype on typerelation.specificationtypeid = specificationtype.id
            join relationtype on typerelation.relationtypeid = relationtype.id and relationtype.name = 'AllowSpecification') as typerel on comptype.id = typerel.componenttypeid";

            var resultIds = connection.Query<ComponentType, ComponentTypeAttribute, Attribute, TypeRelation, SpecificationType, Guid>(query,
                (componentType, componentTypeAttribute, attribute, typeRelation, specificationType) =>
                {
                    if (componentTypeDictionary.ContainsKey(componentType.Id))
                    {
                        if (componentTypeAttribute != null && !componentTypeDictionary[componentType.Id].ComponentTypeAttributes.Any(t => t.Name == attribute.Name))
                        {
                            componentTypeAttribute.Name = attribute.Name;
                            componentTypeDictionary[componentType.Id].ComponentTypeAttributes
                                .Add(componentTypeAttribute);
                        }

                        if (typeRelation != null && typeRelation.SpecificationTypeId != Guid.Empty && !componentTypeDictionary[componentType.Id].SpecificationTypeNames.Contains(specificationType.Name))
                            componentTypeDictionary[componentType.Id].SpecificationTypeNames.Add(specificationType.Name);
                    }
                    else
                    {
                        if (componentTypeAttribute != null)
                        {
                            componentTypeAttribute.Name = attribute.Name;
                            componentType.ComponentTypeAttributes.Add(componentTypeAttribute);
                        }

                        if (typeRelation != null && typeRelation.SpecificationTypeId != Guid.Empty) componentType.SpecificationTypeNames.Add(specificationType.Name);
                        componentTypeDictionary.Add(componentType.Id, componentType);
                    }
                    return componentType.Id;
                }, transaction: tr, commandType: CommandType.Text, splitOn: "id");

            result = componentTypeDictionary.Select(t => t.Value).ToList();

            return result;
        }

        public static Guid GetComponentTypeIdByName(IDbConnection connection, IDbTransaction tr, string componentTypeName)
        {
            var query = "select id from componenttype where name=@Name";
            var result = connection.Query<Guid>(query, new {Name = componentTypeName}, commandType: CommandType.Text);

            return result.FirstOrDefault();
        }


        public static string GetComponentTypeNameById(IDbConnection connection, IDbTransaction tr, Guid dbComponentTypeId)
        {
            var query = @"select name from componenttype where id=@Id";

            var result = connection.Query<string>(query, new {Id = dbComponentTypeId}, commandType: CommandType.Text);

            return result.FirstOrDefault();
        }



        public static Guid GetComponentTypeAttributeId(IDbConnection connection, IDbTransaction tr, Guid dbComponentTypeId, string attributeName)
        {
            var query =
                @"select comptypeattr.id from componenttypeattribute as comptypeattr join attribute as attr on comptypeattr.attributeid = attr.id where comptypeattr.componenttypeid = @TypeId and attr.name = @AttrName";
            var result = connection.Query<Guid>(query, new {TypeId = dbComponentTypeId, AttrName = attributeName},
                commandType: CommandType.Text);

            return result.FirstOrDefault();
        }

        public static Guid GetComponentAttributeId(IDbConnection connection, IDbTransaction tr, Guid componentId, Guid componentTypeAttributeId)
        {
            var query =
                @"select compattr.id from componentattribute as compattr where componentid = @ComponentId and componenttypeattributeid = @ComponentTypeAttributeId";
            var result = connection.Query<Guid>(query,
                new {ComponentId = componentId, ComponentTypeAttributeId = componentTypeAttributeId});

            return result.FirstOrDefault();
        }

        public static void DeleteComponents(IDbConnection connection, IDbTransaction tr, Guid[] componentIDs)
        {
			if (connection == null) throw new ArgumentNullException(nameof(connection));
			var param = new {comps = componentIDs};
			connection.Query("delete from componentattribute where componentid = any (@comps) ", param, tr);
			connection.Query("delete from component where id = any (@comps)", param, tr);
		}

        public static DateTime GetCreateDateByComponentId(IDbConnection connection, IDbTransaction tr, Guid apiComponentId)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            return connection.QuerySingle<DateTime>("select createdate from component where id = @Id",
                new { Id = apiComponentId });
        }

        public static bool CheckAttributeName(IDbConnection connection, IDbTransaction tr, string searchParamName)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query = "select id from Attribute where name=@name;";

            var result = connection.QuerySingleOrDefault<Guid>(query, new {name = searchParamName}); // либо только 1 либо нет

            return result != Guid.Empty;
        }

        public static List<Guid> GetSpecificationTypesForComponentType(Guid componentTypeId, IDbConnection connection, IDbTransaction tr)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query = @"select specificationtype.id from specificationtype left join typerelation on specificationtype.id = typerelation.specificationtypeid left join relationtype on typerelation.relationtypeid = relationtype.id
                            where typerelation.componenttypeid = @Id and relationtype.name = 'AllowSpecification'";

            var result = connection.Query<Guid>(query, new {Id = componentTypeId}, transaction: tr,
                commandType: CommandType.Text).ToList();

            return result;
        }

        public static void AddTypeRelations(IDbConnection connection, IDbTransaction tr, List<TypeRelation> typeRelations)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query =
                @"insert into typerelation values (@Id, @ComponentTypeId, @SpecificationTypeId, @RelationTypeId)";

            connection.Execute(query, typeRelations, tr);
        }

        public static Guid GetTypeRelationIdBySpecificationName(IDbConnection connection, IDbTransaction tr, Guid id, string specificationTypeName)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query =
                @"select typerelation.id from typerelation join specificationtype on typerelation.specificationtypeid = specificationtype.id join relationtype on typerelation.relationtypeid = relationtype.id  where typerelation.componenttypeid = @ComponentTypeId and specificationtype.name = @SpecificationTypeName and relationtype.name = 'AllowSpecification'";

            var result = connection.Query<Guid>(query, new { ComponentTypeId = id, SpecificationTypeName = specificationTypeName }, transaction: tr,
                commandType: CommandType.Text).ToList();

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Метод получения списка удаленных связей с типами спецификаций для типа компонента для их последующего удаления из БД
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tr"></param>
        /// <param name="updatedComponentTypes">Список типов компонентов, для которых идет поиск удаляемых связей</param>
        /// <returns></returns>
        public static List<TypeRelation> GetDeletedTypeRelations(IDbConnection connection, IDbTransaction tr, List<ComponentTypeObject> updatedComponentTypes)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var result = new List<TypeRelation>();

            var query = @"select typerelation.* from typerelation " +
                        "join specificationtype on typerelation.specificationtypeid = specificationtype.id " +
                        "join relationtype on typerelation.relationtypeid = relationtype.id " +
                        "where specificationtype.name not in @SpecificationNames and typerelation.componenttypeid = @ComponentId and relationtype.name = 'AllowSpecification'";

            foreach (var updatedComponentType in updatedComponentTypes)
            {
                var typesRow = string.Empty;

                var currentQuery = query;

                if (updatedComponentType.SpecificationTypeNames.Count > 0)
                {
                    typesRow = "(" + string.Join(", ", updatedComponentType.SpecificationTypeNames.Select(t => $"'{t}'")) + ")";

                    currentQuery = currentQuery.Replace("@SpecificationNames", typesRow);
                }
                else
                {
                    currentQuery = currentQuery.Replace("specificationtype.name not in @SpecificationNames and ", string.Empty);
                }

                var existTypeRelations = connection.Query<TypeRelation>(currentQuery, new { ComponentId = updatedComponentType.ID}, tr);

                result.AddRange(existTypeRelations);
            }

            return result;
        }

        public static void DeleteTypeRelations(IDbConnection connection, IDbTransaction tr, List<TypeRelation> deletedTypeRelations)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            //var ids = deletedTypeRelations.Select(t => t.Id).ToList();
            var query = "delete from typerelation where componenttypeid = @ComponentTypeId and specificationtypeid = @SpecificationTypeId and relationtypeid = @RelationTypeId";
            connection.Execute(query, deletedTypeRelations, tr);
        }

        public static void ClearTypeRelations(IDbConnection connection, IDbTransaction tr)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Query("truncate from typerelation", tr);
        }

        public static Guid GetComponentTypeIdByComponentId(IDbConnection connection, IDbTransaction tr, Guid componentId)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query =
                "select typeid from component where component.id = @Id";
            var typeId = connection.Query<Guid>(query, new {Id = componentId}, tr).FirstOrDefault();

            return typeId;
        }

        public static IEnumerable<Attribute> GetAttributes(IDbConnection connection, IDbTransaction tr)
        {
            var attributeDictionary = new Dictionary<Guid, Attribute>();
            var query = @"select * from attribute join attributetype on attribute.attrtypeid = attributetype.id where attribute.isdeleted is not true order by attribute.name";

            connection.Query<Attribute, AttributeType, Guid>(query, (attribute, attributeType) =>
                {
                    if (!attributeDictionary.ContainsKey(attribute.ID))
                    {
                        attribute.AttributeTypeName = attributeType.Name;
                        attributeDictionary.Add(attribute.ID, attribute);
                    }

                    return attribute.ID;
                }, transaction: tr,
                commandType: CommandType.Text, splitOn: "id");

            var result = attributeDictionary.Select(t => t.Value).ToList();

            return result;
        }

        public static Guid GetAttributeTypeIdByName(IDbConnection connection, IDbTransaction tr, string typeName)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query = @"select id from attributetype where name = @Name";

            var typeId = connection.Query<Guid>(query, new { Name = typeName }, tr).FirstOrDefault();

            return typeId;
        }

        public static void UpdateAttributes(IDbConnection connection, IDbTransaction tr, List<Attribute> updatedDbAttributes)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query =
                "update attribute set name = @Name, attrtypeid = @AttrTypeId, datatype = @DataType, \"unique\" = @Unique where id = @ID";

            connection.Execute(query, updatedDbAttributes, tr);

        }

        public static void DeleteAttributes(IDbConnection connection, IDbTransaction tr,
            List<Attribute> dbDeletedAttributes)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var query = @"delete from attribute where id=@ID";//@"update attribute set isdeleted=@IsDeleted where id=@ID";

            connection.Execute(query, dbDeletedAttributes, tr);
        }


        public static bool CheckUniqueComponent(IDbConnection connection, IDbTransaction tr, Component component)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var result = true;

            var componentTypes = GetComponentTypes(connection, tr);

            var foundComponentType = componentTypes.FirstOrDefault(t => t.Id == component.TypeId);
            if (foundComponentType != null)
            {
                foreach (var componentAttribute in component.ComponentAttributes)
                {
                    
                }
            }
            

            

            return result;
        }


        public static bool IsAttributeValueUnique(IDbConnection connection, IDbTransaction tr, Guid attributeId, string attributeValue)
        {
            var result = false;

            var query =
                @"select count(*) from componentattribute join componenttypeattribute on componentattribute.componenttypeattributeid = componenttypeattribute.id
where componenttypeattribute.attributeid = @AttributeId and componentattribute.value = @AttributeValue";

            var count = connection.Query<int>(query, new { AttributeId = attributeId, AttributeValue = attributeValue }, tr).FirstOrDefault();

            return count == 0;

        }

        public static bool IsAllowDeleteComponentType(IDbConnection connection, IDbTransaction tr, Guid componentTypeId)
        {
            var result = false;

            var query = "select exists(select 1 from component where typeid = @TypeId);";//"select id from component where typeid = @TypeId";

            var queryResult = connection.Query<bool>(query, new {TypeId = componentTypeId}, tr);

            return !queryResult.FirstOrDefault();
        }

        public static void DeleteComponentTypes(IDbConnection connection, IDbTransaction tr, List<ComponentType> deletedDbComponentTypes)
        {
            var query = "delete from componenttype where id = @Id";

            var deleteTypeRelationsQuery = "delete from typerelation where componenttypeid = @Id";

            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Execute(deleteTypeRelationsQuery, deletedDbComponentTypes, tr);
            connection.Execute(query, deletedDbComponentTypes, tr);
        }

        public static void DeleteComponentTypeAttributes(IDbConnection connection, IDbTransaction tr, List<ComponentTypeAttribute> deletedComponentTypeAttributes)
        {
            var query = "delete from componenttypeattribute where id = @Id";

            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Execute(query, deletedComponentTypeAttributes, tr);
        }

        public static void DeleteTypeRelationsForComponentType(IDbConnection connection, IDbTransaction tr, List<ComponentType> deletedDbComponentTypes)
        {
            var query = "delete from typerelation where componenttypeid = @Id";

            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection.Execute(query, deletedDbComponentTypes, tr);
        }
    }
}
