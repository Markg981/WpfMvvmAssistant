using System.Collections.ObjectModel;
using Core.Domain.Models;
using Serilog;

namespace UI.Wpf.ViewModels;

public class SchemaBrowserViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private DatabaseSchema? _schema;
    private ObservableCollection<SchemaTreeItem> _treeItems = new();

    public SchemaBrowserViewModel(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public DatabaseSchema? Schema
    {
        get => _schema;
        set
        {
            if (SetProperty(ref _schema, value))
            {
                BuildTree();
            }
        }
    }

    public ObservableCollection<SchemaTreeItem> TreeItems
    {
        get => _treeItems;
        set => SetProperty(ref _treeItems, value);
    }

    private void BuildTree()
    {
        TreeItems.Clear();

        if (Schema == null)
            return;

        var serverNode = new SchemaTreeItem
        {
            Name = Schema.ServerName,
            Type = SchemaItemType.Server,
            Icon = "Server"
        };

        foreach (var database in Schema.Databases)
        {
            var dbNode = new SchemaTreeItem
            {
                Name = database.Name,
                Type = SchemaItemType.Database,
                Icon = "Database"
            };

            foreach (var schema in database.Schemas)
            {
                var schemaNode = new SchemaTreeItem
                {
                    Name = schema.Name,
                    Type = SchemaItemType.Schema,
                    Icon = "Folder"
                };

                if (schema.Tables.Any())
                {
                    var tablesFolder = new SchemaTreeItem
                    {
                        Name = "Tables",
                        Type = SchemaItemType.Folder,
                        Icon = "FolderTable"
                    };

                    foreach (var table in schema.Tables)
                    {
                        var tableNode = new SchemaTreeItem
                        {
                            Name = table.TableName,
                            Type = SchemaItemType.Table,
                            Icon = "Table",
                            Tag = table
                        };

                        foreach (var column in table.Columns)
                        {
                            tableNode.Children.Add(new SchemaTreeItem
                            {
                                Name = $"{column.ColumnName} ({column.DataType})",
                                Type = SchemaItemType.Column,
                                Icon = column.IsPrimaryKey ? "Key" : "Column",
                                Tag = column
                            });
                        }

                        tablesFolder.Children.Add(tableNode);
                    }

                    schemaNode.Children.Add(tablesFolder);
                }

                if (schema.Views.Any())
                {
                    var viewsFolder = new SchemaTreeItem
                    {
                        Name = "Views",
                        Type = SchemaItemType.Folder,
                        Icon = "FolderView"
                    };

                    foreach (var view in schema.Views)
                    {
                        viewsFolder.Children.Add(new SchemaTreeItem
                        {
                            Name = view.ViewName,
                            Type = SchemaItemType.View,
                            Icon = "View",
                            Tag = view
                        });
                    }

                    schemaNode.Children.Add(viewsFolder);
                }

                if (schema.StoredProcedures.Any())
                {
                    var spFolder = new SchemaTreeItem
                    {
                        Name = "Stored Procedures",
                        Type = SchemaItemType.Folder,
                        Icon = "FolderProc"
                    };

                    foreach (var sp in schema.StoredProcedures)
                    {
                        spFolder.Children.Add(new SchemaTreeItem
                        {
                            Name = sp.ProcedureName,
                            Type = SchemaItemType.StoredProcedure,
                            Icon = "StoredProcedure",
                            Tag = sp
                        });
                    }

                    schemaNode.Children.Add(spFolder);
                }

                if (schema.Functions.Any())
                {
                    var funcFolder = new SchemaTreeItem
                    {
                        Name = "Functions",
                        Type = SchemaItemType.Folder,
                        Icon = "FolderFunc"
                    };

                    foreach (var func in schema.Functions)
                    {
                        funcFolder.Children.Add(new SchemaTreeItem
                        {
                            Name = func.FunctionName,
                            Type = SchemaItemType.Function,
                            Icon = "Function",
                            Tag = func
                        });
                    }

                    schemaNode.Children.Add(funcFolder);
                }

                dbNode.Children.Add(schemaNode);
            }

            serverNode.Children.Add(dbNode);
        }

        TreeItems.Add(serverNode);
    }
}

public class SchemaTreeItem : ViewModelBase
{
    private string _name = string.Empty;
    private string _icon = "Folder";
    private SchemaItemType _type;
    private object? _tag;
    private ObservableCollection<SchemaTreeItem> _children = new();

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public SchemaItemType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public object? Tag
    {
        get => _tag;
        set => SetProperty(ref _tag, value);
    }

    public ObservableCollection<SchemaTreeItem> Children
    {
        get => _children;
        set => SetProperty(ref _children, value);
    }
}

public enum SchemaItemType
{
    Server,
    Database,
    Schema,
    Folder,
    Table,
    Column,
    View,
    StoredProcedure,
    Function
}
