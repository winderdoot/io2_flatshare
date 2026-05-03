import { useQuery } from "@tanstack/react-query";
import { useFiltersStore } from "../SearchBar/FiltersStore";
import { API_URL } from "../../config";

export interface Listing {
  id: string;
  title: string;
  price: number;
  city: string;
  district: string;
  area: number;
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
): Promise<any> => {
  const query = buildQueryParams(filters, page);
  console.log(query);
  
  const res = await fetch(`${API_URL}/api/v1/matches?${query}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
    })

    
    if (!res.ok) {
      throw new Error("Błąd pobierania ogłoszeń");
    }
    
  // const data = await res.json();
  // console.log(data);
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