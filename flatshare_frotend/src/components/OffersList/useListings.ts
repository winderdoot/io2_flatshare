import { useQuery } from "@tanstack/react-query";
import { useFiltersStore } from "../SearchBar/FiltersStore";

export interface Listing {
  id: string;
  title: string;
  price: number;
  city: string;
  district: string;
  area: number;
}

export interface ListingsResponse {
  items: Listing[];
  total: number;
  page: number;
  pageSize: number;
}

const buildQueryParams = (filters: Record<string, any>, page: number) => {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value === "" || value === false) return;
    params.append(key, String(value));
  });

  params.append("page", String(page));

  return params.toString();
};

const fetchListings = async (
  filters: Record<string, any>,
  page: number
): Promise<ListingsResponse> => {
    console.log("poszło query");
    console.log(filters);
  const query = buildQueryParams(filters, page);

  const res = await fetch(`/api/listings?${query}`);

  if (!res.ok) {
    throw new Error("Błąd pobierania ogłoszeń");
  }

  return res.json();
};


export const useListings = () => {
  const { appliedFilters, page } = useFiltersStore();

  return useQuery({
    queryKey: ["listings", appliedFilters, page],
    queryFn: () => fetchListings(appliedFilters, page),
    staleTime: 1000 * 30,
    refetchOnWindowFocus: false
  });
};