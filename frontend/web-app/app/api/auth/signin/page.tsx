import EmptyFilter from "@/app/components/EmptyFilter";

export default function SignIn({
  searchParams,
}: {
  searchParams: { callbackUrl: string };
}) {
  return (
    <EmptyFilter
      subtile="Please click below to login"
      title="You need to be logged in to view this page"
      showLogin
      callbackUrl={searchParams.callbackUrl}
    />
  );
}
