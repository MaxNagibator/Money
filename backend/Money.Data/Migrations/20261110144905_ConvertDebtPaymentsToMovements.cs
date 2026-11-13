using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Migrations;

public partial class ConvertDebtPaymentsToMovements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TEMP TABLE legacy_debt_payments ON COMMIT DROP AS
            SELECT
                d.user_id,
                u.next_debt_id + ROW_NUMBER() OVER (PARTITION BY d.user_id ORDER BY d.id)::integer - 1 AS id,
                CASE d.type_id WHEN 1 THEN 2 ELSE 1 END AS type_id,
                d.pay_sum AS sum,
                NULLIF(d.pay_comment, '') AS comment,
                d.date,
                d.owner_id,
                d.is_deleted
            FROM debts d
            INNER JOIN domain_users u ON u.id = d.user_id
            WHERE d.pay_sum > 0;

            INSERT INTO debts
                (user_id, id, type_id, sum, comment, date, owner_id, is_deleted,
                 pay_sum, pay_comment, status_id)
            SELECT
                user_id, id, type_id, sum, comment, date, owner_id, is_deleted,
                0, NULL, 1
            FROM legacy_debt_payments;

            UPDATE domain_users user_row
            SET next_debt_id = user_row.next_debt_id + movements.count
            FROM (
                SELECT user_id, COUNT(*)::integer AS count
                FROM legacy_debt_payments
                GROUP BY user_id
            ) movements
            WHERE user_row.id = movements.user_id;

            ALTER TABLE debts DROP COLUMN pay_sum;
            ALTER TABLE debts DROP COLUMN pay_comment;
            ALTER TABLE debts DROP COLUMN status_id;
            ALTER TABLE debts RENAME TO debt_movements;
            ALTER TABLE debt_movements RENAME CONSTRAINT pk_debts TO pk_debt_movements;
            ALTER TABLE debt_movements RENAME CONSTRAINT fk_debts_debt_owners_user_id_owner_id TO fk_debt_movements_debt_owners_user_id_owner_id;
            ALTER TABLE debt_movements RENAME CONSTRAINT fk_debts_domain_users_user_id TO fk_debt_movements_domain_users_user_id;
            ALTER INDEX ix_debts_user_id_owner_id RENAME TO ix_debt_movements_user_id_owner_id;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER INDEX ix_debt_movements_user_id_owner_id RENAME TO ix_debts_user_id_owner_id;
            ALTER TABLE debt_movements RENAME CONSTRAINT fk_debt_movements_domain_users_user_id TO fk_debts_domain_users_user_id;
            ALTER TABLE debt_movements RENAME CONSTRAINT fk_debt_movements_debt_owners_user_id_owner_id TO fk_debts_debt_owners_user_id_owner_id;
            ALTER TABLE debt_movements RENAME CONSTRAINT pk_debt_movements TO pk_debts;
            ALTER TABLE debt_movements RENAME TO debts;
            ALTER TABLE debts ADD COLUMN pay_sum numeric NOT NULL DEFAULT 0;
            ALTER TABLE debts ADD COLUMN pay_comment character varying(4000) NULL;
            ALTER TABLE debts ADD COLUMN status_id integer NOT NULL DEFAULT 1;
            """);
    }
}
