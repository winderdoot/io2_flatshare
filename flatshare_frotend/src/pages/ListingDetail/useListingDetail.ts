import { useQuery } from "@tanstack/react-query";
import { landlordListingsService } from "../LandlordListings/LandlordListingsService";

export const useListingDetail = (listingId: string | undefined) => {
  return useQuery({
    queryKey: ["listing", listingId],
    queryFn: () => landlordListingsService.getById(listingId!),
    enabled: !!listingId,
    staleTime: 1000 * 60,
    retry: 1,
  });
};
