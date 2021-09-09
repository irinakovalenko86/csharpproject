using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using NLog;
using SwrElectricaData.Data;
using SwrElectricaData.Data.CommonDataBaseModel;
using SwrElectricaData.Data.Enum;
using SwrElectricaData.Data.Settings;
using SwrElectricaData.Logic.DataBases.Sdf;
using SwrElectricaData.Logic.Databases.Pdm;
using SwrElectricaData.Logic.Registry;
using SwrElectricaData.Logic.SwePdm;
using SwrElectricaData.Properties;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SwrElectricaData.Logic.DataBases
{
    public class MaterialRepositoryBuilder
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public MaterialRepository Build(ElectricaSettings electricaSettings, MaterialType materialType, bool includeAnnul, string profileID, bool needShowError)
        {
            MaterialRepository materialRepository = null;

            DataSourceProfile profile = electricaSettings.DataSourceSettings.GetProfileById(profileID);

			if (profile == null)
			{
				if (needShowError)
				{
					MsgBox.Show(Resources.DataProfileDoesNotExist, MessageBoxButton.OK, MessageBoxImage.Error);	
				}
                return materialRepository;
            }

            string message = string.Empty;

            var dataSourceAnalizer = new DataSourceAnalizer();
            var result = dataSourceAnalizer.AnalizeDataSource(profile, out message);

			if (!result && needShowError)
            {
                logger.Error(message);

                MsgBox.Show(message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (profile.Type == DataSourceType.SwePdm)
                {
                    try
                    {
                        var dataBaseProfile = profile as DataBaseProfile;

                        if (dataBaseProfile != null)
                        {
                            var pdmController = new PdmController();

                            string vaultName = pdmController.GetVaultNameByPath(dataBaseProfile.Path);

                            var registryService = new RegistryService();
                            var registryController = new RegistryController(registryService);

                            string dbName = "";
                            var serverName = registryController.GetDBName(vaultName, out dbName);

                            var databaseParameters = new DatabaseParameters()
                                                            {
                                                                DataSource = serverName,
                                                                InitialCatalog = dbName,
                                                                UserID = dataBaseProfile.UserName,
                                                                Password = dataBaseProfile.Password
                                                            };

                            var rootFolderPath  = pdmController.GetRootFolderOfVault(vaultName);

                            var folder = dataBaseProfile.Path.Substring(rootFolderPath.Count());

                            var pdmDBService = new PdmDBService();

                            var dataContext = pdmDBService.GetDbContext(databaseParameters);
                            var pdmModelBuilder = new PdmModelBuilder();

                            MaterialRepository materialRepos = null;

                            materialRepos = pdmModelBuilder.Build(dataContext, folder, materialType,
                                                                    dataBaseProfile.UseLastReduction,
                                                                    dataBaseProfile.AnnulVariableName,
                                                                    includeAnnul);



                            materialRepository = materialRepos; 
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(Resources.PdmDataBaseError, ex);
	                    if (needShowError)
	                    {
		                    MsgBox.Show(Resources.PdmDataBaseError, MessageBoxButton.OK, MessageBoxImage.Error);
	                    }
                    }
                }

                if (profile.Type == DataSourceType.SdfFile)
                {
                    try
                    {
                        var dataBasePath = profile.Path;

                        var sdfDBService = new SdfDBService();

                        using (var connection = sdfDBService.GetConnection(dataBasePath))
                        {
                            var sdfModelBuilder = new SdfModelBuilder();

                            MaterialRepository materialRepos = null;

                            materialRepos = sdfModelBuilder.Build(connection, materialType, includeAnnul);

                            materialRepository = materialRepos;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(Resources.SdfDataBaseError, ex);
						if (needShowError)
	                    {
							MsgBox.Show(Resources.SdfDataBaseError, MessageBoxButton.OK, MessageBoxImage.Error);
	                    }
                    }
                }
            }

            if (materialRepository!=null && materialRepository.Materials.Count == 0)
            {
                logger.Warn("Профиль данных " + profile.Name + "(" + profile.Path + ") не найдены материалы. Тип материала: " + materialType + ", выбирать аннулированные: " + includeAnnul);
                MessageBox.Show("В указанном источнике данных не обнаружены материалы.", 
                    Constants.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                materialRepository = null;
            }

            return materialRepository;
        }

        public ColorRepository BuildColorRepository(ElectricaSettings electricaSettings)
        {
            //TODO DB использовать DataSourceAnalizer как в функции Build
            //TODO DB понять нужны ли вообще репозитории цветов
            ColorRepository colorRepository = null;

            var defaultProfile = electricaSettings.DataSourceSettings.GetDefaultProfile();

            if (defaultProfile.Type == DataSourceType.SwePdm)
            {
                try
                {
                    var dataBaseProfile = defaultProfile as DataBaseProfile;

                    if (dataBaseProfile != null)
                    {
                        string vaultName = string.Empty;

                        var pdmController = new SwePdmServices();

                        if (pdmController.ObjectSaveInSwePdm(dataBaseProfile.Path, out vaultName))
                        {
                            var registryService = new RegistryService();
                            var registryController = new RegistryController(registryService);

                            string dbName = "";
                            var serverName = registryController.GetDBName(vaultName, out dbName);

                            var databaseParameters = new DatabaseParameters()
                            {
                                DataSource = serverName,
                                InitialCatalog = dbName,
                                UserID = dataBaseProfile.UserName,
                                Password = dataBaseProfile.Password
                            };

                            string warning = string.Empty;

                            var pdmDBService = new PdmDBService();
                            var isConnectionAvaliable = pdmDBService.IsConnectionAvaliable(databaseParameters, out warning);

                            if (isConnectionAvaliable)
                            {
                                var dataContext = pdmDBService.GetDbContext(databaseParameters);
                                var pdmModelBuilder = new PdmModelBuilder();

                                ColorRepository colorRepos = pdmModelBuilder.BuildColorRepository(dataContext);

                                colorRepository = colorRepos;
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    logger.ErrorException(Resources.PdmDataBaseError, ex);
                    MessageBox.Show(Resources.PdmDataBaseError, Constants.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }


            if (defaultProfile.Type == DataSourceType.SdfFile)
            {
                var dataBasePath = defaultProfile.Path;

                var sdfDBService = new SdfDBService();

                var connection = sdfDBService.GetConnection(dataBasePath);

                var sdfModelBuilder = new SdfModelBuilder();

                ColorRepository colorRepos = sdfModelBuilder.BuildColorRepository(connection);
                

                colorRepository = colorRepos;
            }


            return colorRepository;
        }
    }
}
