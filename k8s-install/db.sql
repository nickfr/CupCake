
--drop database CupCake;

--create database CupCake owner postgres;


--create user cupcake_svc with password 'cupcakesvc123';

CREATE TABLE public.test_data (id SERIAL PRIMARY KEY, data TEXT NOT NULL);
INSERT INTO public.test_data (data)
SELECT md5(random()::TEXT)
FROM generate_series(1, 1000000) s(i);
CREATE OR REPLACE FUNCTION public.get_data()
RETURNS SETOF public.test_data
LANGUAGE plpgsql
STABLE
AS $$
BEGIN
  RETURN QUERY
  SELECT id, data
  FROM public.test_data;
END
$$;

GRANT SELECT ON TABLE public.test_data TO cupcake_svc;
GRANT EXECUTE ON FUNCTION public.get_data() TO cupcake_svc;
