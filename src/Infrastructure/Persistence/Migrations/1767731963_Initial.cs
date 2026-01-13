using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace Infrastructure.Persistence.Migrations;

#pragma warning disable SA1649
[Migration(1767731963, "Initial")]
public class InitialMigration : IMigration
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
                           create table if not exists roles
                           (
                               role_id              bigint primary key generated always as identity,
                               code                 text unique not null
                           );

                           create table if not exists account_passwords
                           (
                               password_id          bigint primary key generated always as identity,
                               password_hash        text not null
                           );

                           create table if not exists accounts
                           (
                               account_id           bigint primary key generated always as identity,

                               role_id              bigint not null references roles (role_id),
                               password_id          bigint not null references account_passwords (password_id),
                               email                text unique not null,
                               account_created_at   timestamp with time zone not null,
                               account_updated_at   timestamp with time zone not null
                           );

                           create table if not exists student_profiles
                           (
                               account_id           bigint primary key references accounts (account_id),
                           
                               nickname             text unique not null,
                               profile_photo_url    text
                           );

                           create table if not exists followers
                           (
                               follower_id          bigint references accounts (account_id),
                               followee_id          bigint references accounts (account_id),
                               created_at           timestamp with time zone not null,
                               primary key (follower_id, followee_id)
                           );
                           """,
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
                           drop table if exists followers;
                           drop table if exists student_profiles;
                           drop table if exists accounts;
                           drop table if exists account_passwords;
                           drop table if exists roles;
                           """,
        });
    }

    public string ConnectionString => throw new NotSupportedException();
}