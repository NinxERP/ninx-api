using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ninx.Tests.Integration
{
    /// <summary>
    /// As configurações de mapeamento em ninx.Data/Mapping (ex.: AssinaturaEletronicaMapping,
    /// ComercioMapping) usam <c>HasColumnType("nvarchar(max)")</c> / <c>"decimal(10,2)"</c> — tipos de
    /// coluna explícitos do SQL Server, aplicados incondicionalmente em NinxDB.OnModelCreating.
    /// O SQLite não entende "nvarchar(max)" (gera "near 'max': syntax error" ao criar o schema).
    /// <para>
    /// Em vez de alterar o código de mapeamento de produção para condicionar esses tipos por provider
    /// (fora do escopo deste trabalho — ver nota de testabilidade no relatório final), este
    /// <see cref="IModelCustomizer"/> customizado roda depois de OnModelCreating e remove os
    /// HasColumnType explícitos do modelo em memória usado pelos testes, deixando o SQLite inferir o
    /// tipo de coluna a partir do tipo CLR da propriedade.
    /// </para>
    /// </summary>
    public class SqliteColumnTypeStrippingModelCustomizer : ModelCustomizer
    {
        public SqliteColumnTypeStrippingModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnType(null);

                    // Estoque.RowVersion (e afins) usam .IsRowVersion(), pensado para o tipo nativo
                    // ROWVERSION/TIMESTAMP do SQL Server, gerado automaticamente pelo banco a cada
                    // INSERT/UPDATE. SQLite não gera esse valor sozinho: como a propriedade fica marcada
                    // como "gerada pelo banco", o EF simplesmente não a envia no INSERT, e a coluna
                    // NOT NULL falha. Para os testes, tratamos como um valor comum (definido pela
                    // aplicação/seed), abrindo mão apenas da geração automática pelo SQLite.
                    if (property.IsConcurrencyToken && property.ValueGenerated != ValueGenerated.Never)
                    {
                        property.ValueGenerated = ValueGenerated.Never;
                    }
                }
            }
        }
    }
}
